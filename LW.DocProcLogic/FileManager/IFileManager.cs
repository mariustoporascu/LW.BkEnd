﻿using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LW.BkEndModel;
using LW.DocProcLogic.DbRepo;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LW.BkEndModel.Enums;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using LW.DocProcLogic.ProcessOcrResult;
using Azure.AI.FormRecognizer.Models;
using SkiaSharp;
using System.IO;
using System.Net.Http;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Interactive;
using Microsoft.Extensions.Logging;

namespace LW.DocProcLogic.FileManager
{
    public interface IFileManager
    {
        Task<bool> OnFileUpload(FormFile formFile, Guid conexId, Guid firmaDiscId);
        Task<bool> OnFileRescan(FormFile formFile, Guid conexId, Guid documentId);
        Task<bool> OnFileProcessed(string blobName, string result);
        Task<Stream> GetFileStream(string identifier, Guid conexId);
    }

    public class FileManager : IFileManager
    {
        private readonly IConfiguration _config;
        private readonly IDbRepo _dbRepo;
        private readonly ILogger<FileManager> _logger;

        public FileManager(IConfiguration config, IDbRepo dbRepo, ILogger<FileManager> logger)
        {
            _config = config;
            _dbRepo = dbRepo;
            _logger = logger;
        }

        public async Task<Stream> GetFileStream(string identifier, Guid conexId)
        {
            var exists = _dbRepo.GetDocumentByBlobName(identifier);
            var conexCont = _dbRepo.GetConexCont(conexId);
            if (exists == null || conexCont == null)
            {
                return null;
            }
            if (
                exists.FirmaDiscountId == conexCont.FirmaDiscountId
                || exists.ConexiuniConturi.HybridId == conexCont.HybridId
                || exists.ConexId == conexId
            )
            {
                var blobClient = new BlobClient(
                    _config["Azure:Storage"],
                    _config["Azure:ContainerName"],
                    identifier
                );
                var response = await blobClient.DownloadContentAsync();
                if (exists.FisiereDocumente.FileExtension.Contains("image"))
                {
                    PdfBitmap image = new PdfBitmap(response.Value.Content.ToStream());

                    //Create a new PDF document
                    PdfDocument doc = new PdfDocument();
                    if (image.Width > image.Height)
                    {
                        doc.PageSettings.Orientation = PdfPageOrientation.Landscape;
                    }
                    //Add a page to the document
                    PdfPage page = doc.Pages.Add();

                    //Create PDF graphics for the page
                    PdfGraphics graphics = page.Graphics;
                    //Load the image from the disk
                    //Draw the image

                    graphics.DrawImage(image, 0, 0);
                    var stream = new MemoryStream();
                    //Save the document
                    doc.Save(stream);
                    //Close the document
                    doc.Close(true);
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream;
                }
                else
                {
                    return response.Value.Content.ToStream();
                }
            }
            return null;
        }

        public async Task<bool> OnFileProcessed(string blobName, string result)
        {
            //return false;
            var dbFile = _dbRepo.GetDocumentByBlobName(blobName);
            var dbFirmaDisc = _dbRepo.GetFirmaDiscountById(dbFile.FirmaDiscountId ?? Guid.Empty);
            if (dbFile == null || dbFirmaDisc == null)
            {
                return false;
            }
            try
            {
                JObject processedResult = JsonConvert.DeserializeObject<JObject>(result);
                if (!dbFile.IsInvoice)
                {
                    ResultProcessor.ProcessReceiptForFileManager(
                        ref dbFile,
                        dbFirmaDisc,
                        processedResult
                    );
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                dbFile.Status = (int)StatusEnum.FailedProcessing;
                dbFile.StatusName = StatusEnum.FailedProcessing.ToString();
                _logger.LogWarning(ex.Message);
            }

            return await _dbRepo.UpdateCommonEntity(dbFile);
        }

        public async Task<bool> OnFileRescan(FormFile formFile, Guid conexId, Guid documentId)
        {
            var exists = _dbRepo.CheckDocExists(conexId, documentId);
            if (!exists)
                return false;
            try
            {
                HttpClient _httpClient = new HttpClient();
                using Stream stream = formFile.OpenReadStream();
                // get document number from barcode
                string barCode = string.Empty;
                var imageContent = new StreamContent(stream);
                var barCodeResult = await _httpClient.PostAsync(
                    _config["BarCodeEndpointZbar"],
                    imageContent
                );
                if (barCodeResult.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    barCodeResult = await _httpClient.PostAsync(
                        _config["BarCodeEndpointZxing"],
                        imageContent
                    );
                }
                if (barCodeResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    barCode = await barCodeResult.Content.ReadAsStringAsync();
                }
                if (string.IsNullOrWhiteSpace(barCode))
                {
                    return false;
                }
                var document = _dbRepo.GetDocumentById(documentId);
                var ocrObject = JsonConvert.DeserializeObject<JObject>(document.OcrDataJson);
                JObject newDocNr = new JObject();

                newDocNr["value"] = JsonConvert.DeserializeObject<JObject>(barCode)[
                    "data"
                ].ToString();
                newDocNr["hasErrors"] = false;

                ocrObject["docNumber"] = newDocNr;
                document.OcrDataJson = JsonConvert.SerializeObject(ocrObject);
                return await _dbRepo.UpdateCommonEntity(document);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return false;
            }
        }

        public async Task<bool> OnFileUpload(FormFile formFile, Guid conexId, Guid firmaDiscId)
        {
            try
            {
                Stream stream = formFile.OpenReadStream();

                var fileExtension = formFile.ContentType;
                if (fileExtension.Contains("image"))
                {
                    PdfDocument doc = new PdfDocument();
                    PdfBitmap image = new PdfBitmap(stream);

                    doc.PageSettings.Orientation =
                        image.Width > image.Height
                            ? PdfPageOrientation.Landscape
                            : PdfPageOrientation.Portrait;
                    PdfPage page = doc.Pages.Add();

                    PdfGraphics graphics = page.Graphics;
                    // draw the image to the PDF page while not exceeding the page bounds
                    graphics.DrawImage(
                        image,
                        0,
                        0,
                        page.GetClientSize().Width,
                        page.GetClientSize().Height
                    );
                    var newStream = new MemoryStream();
                    doc.Save(newStream);
                    doc.Close(true);
                    stream = new MemoryStream(newStream.ToArray());
                    stream.Seek(0, SeekOrigin.Begin);
                }
                var dbFile = new Documente
                {
                    ConexId = conexId,
                    FirmaDiscountId = firmaDiscId,
                    FisiereDocumente = new FisiereDocumente
                    {
                        FileName =
                            $"{formFile.FileName.Substring(0, formFile.FileName.LastIndexOf('.'))}.pdf",
                        FileExtension = "application/pdf",
                    }
                };
                if (!await _dbRepo.AddCommonEntity(dbFile))
                {
                    return false;
                }
                // Create a BlobServiceClient object using the connection string
                BlobServiceClient blobServiceClient = new BlobServiceClient(
                    _config["Azure:Storage"]
                );

                // Get a reference to the blob container
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(
                    _config["Azure:ContainerName"]
                );

                // Get a reference to the blob
                BlobClient blobClient = blobContainerClient.GetBlobClient(
                    dbFile.FisiereDocumente.Identifier
                );
                stream.Seek(0, SeekOrigin.Begin);
                /*if (fileExtension.Contains("image"))
                {
                    MemoryStream resizedImageStream = ResizeImageAndWriteToStream(
                        stream,
                        1080,
                        1080
                    );
                    resizedImageStream.Seek(0, SeekOrigin.Begin);
                    // Upload the file to the blob
                    await blobClient.UploadAsync(
                        resizedImageStream,
                        new BlobUploadOptions
                        {
                            HttpHeaders = new BlobHttpHeaders { ContentType = fileExtension }
                        }
                    );
                }
                else
                {*/
                // Upload the file to the blob
                await blobClient.UploadAsync(
                    stream,
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = "application/pdf" }
                    }
                );
                //}

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                // Handle exceptions
                return false;
            }
        }

        private SKBitmap ResizeImageMaintainAspectRatio(
            SKBitmap originalImage,
            int newWidth,
            int newHeight
        )
        {
            // Calculate the aspect ratio
            float originalAspectRatio = (float)originalImage.Width / originalImage.Height;
            float newAspectRatio = (float)newWidth / newHeight;

            if (originalAspectRatio > newAspectRatio)
            {
                // Keep the width and adjust the height
                newHeight = (int)(newWidth / originalAspectRatio);
            }
            else
            {
                // Keep the height and adjust the width
                newWidth = (int)(newHeight * originalAspectRatio);
            }

            // Create the new empty bitmap with the calculated dimensions
            SKBitmap resizedImage = new SKBitmap(newWidth, newHeight);

            // Draw the original image onto the new bitmap
            using (SKCanvas canvas = new SKCanvas(resizedImage))
            {
                canvas.DrawBitmap(originalImage, new SKRect(0, 0, newWidth, newHeight));
            }

            return resizedImage;
        }

        private MemoryStream ResizeImageAndWriteToStream(
            Stream inputStream,
            int newWidth,
            int newHeight
        )
        {
            MemoryStream outputStream = new MemoryStream();

            using (SKBitmap originalImage = SKBitmap.Decode(inputStream))
            {
                using (
                    SKBitmap resizedImage = ResizeImageMaintainAspectRatio(
                        originalImage,
                        newWidth,
                        newHeight
                    )
                )
                {
                    SKPixmap pixmap = new SKPixmap(resizedImage.Info, resizedImage.GetPixels());
                    if (pixmap.Encode(outputStream, SKEncodedImageFormat.Jpeg, 90))
                    {
                        outputStream.Flush();
                        outputStream.Position = 0; // Reset the position of the MemoryStream to the beginning
                    }
                }
            }

            return outputStream;
        }
    }
}
