// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.Text;
using Azure.Storage.Blobs;
using LW.BkEndModel.Enums;
using LW.DocProcLogic.DbRepo;
using LW.DocProcLogic.MicrosoftOcr;
using LW.DocProcLogic.ProcessOcrResult;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SkiaSharp;
using Syncfusion.EJ2.PdfViewer;

namespace FilesProcessing
{
    public class OnNewFileV2
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly IOcrPrebuilt _ocrPrebuilt;
        private readonly IConfiguration _config;
        private readonly IDbRepo _dbRepo;
        private readonly BlobServiceClient _blobServiceClient;

        public OnNewFileV2(ILoggerFactory loggerFactory, IOcrPrebuilt ocrPrebuilt, IConfiguration config, IDbRepo dbRepo)
        {
            _logger = loggerFactory.CreateLogger<OnNewFileV2>();
            _httpClient = new HttpClient();
            _ocrPrebuilt = ocrPrebuilt;
            _config = config;
            _dbRepo = dbRepo;
            _blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
        }

        [Function("OnNewFileV2")]
        public async Task Run([EventGridTrigger] MyEventGridEvent input)
        {
            string url = input.Data.url;
            string name = Path.GetFileName(new Uri(url).LocalPath);

            _logger.LogInformation($"Processing blob\n Name: {name}");

            // Get Blob Reference
            var blob = _blobServiceClient.GetBlobContainerClient(Environment.GetEnvironmentVariable("BlobContainerName")).GetBlobClient(name);

            if (!await blob.ExistsAsync())
            {
                _logger.LogError($"Blob {name} not found");
                return;
            }

            // Download the blob
            var memoryStream = new MemoryStream();
            await blob.DownloadToAsync(memoryStream);
            var myBlob = memoryStream.ToArray().AsMemory();

            // Convert ReadOnlyMemory<byte> to Stream
            var stream = new MemoryStream(myBlob.ToArray());

            // Update status to Processing && get if invoice or receipt
            await _dbRepo.UpdateBlobStatus(name, StatusEnum.Processing);
            var blobType = _dbRepo.GetBlobType(name)/*false*/;
            var blobFileType = _dbRepo.GetBlobFileType(name)/*"application/pdf"*/;

            if (!blobFileType.Contains("image"))
            {
                PdfRenderer renderer = new PdfRenderer();
                renderer.Load(stream);
                var bitMapimg = renderer.ExportAsImage(0);
                var image = SKImage.FromBitmap(bitMapimg);
                var imageData = image.Encode(SKEncodedImageFormat.Png, 100);
                stream = new MemoryStream();
                imageData.SaveTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }

            if (!blobType)
            {
                // get document number from barcode
                string barCode = string.Empty;
                var imageContent = new StreamContent(stream);

                var barCodeResult = await _httpClient.PostAsync(_config["BarCodeEndpointZbar"], imageContent);
                if (barCodeResult.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    barCodeResult = await _httpClient.PostAsync(_config["BarCodeEndpointZxing"], imageContent);
                }
                if (barCodeResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    barCode = await barCodeResult.Content.ReadAsStringAsync();
                }
                _logger.LogInformation($"blob barcode result: {barCode}");

                stream.Seek(0, SeekOrigin.Begin);
                // Send to OCR
                var analizedResult = await _ocrPrebuilt.SendToOcrAsync(stream, "prebuilt-receipt");
                var processedResult = ResultProcessor.ProcessReceiptForFunctionApp(analizedResult);
                processedResult["docNumber"] = barCode;
                // Send to DB Server
                var objectContent = new
                {
                    BlobName = name,
                    AnalyzeResult = JsonConvert.SerializeObject(processedResult)
                };
                var httpContent = new StringContent(JsonConvert.SerializeObject(objectContent), Encoding.UTF8, "application/json");
                var dbServerResult = await _httpClient.PostAsync(_config["FinalizeEndpoint"], httpContent);
                // Throw exception if not success
                if (!dbServerResult.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {

                // Send to OCR
                AnalyzeResult analizedResult = await _ocrPrebuilt.SendToOcrAsync(stream, "prebuilt-invoice");
                var processedResult = ResultProcessor.ProcessInvoiceForFunctionApp(analizedResult);

                // Send to DB Server
                var objectContent = new
                {
                    BlobName = name,
                    AnalyzeResult = JsonConvert.SerializeObject(processedResult)
                };
                var httpContent = new StringContent(JsonConvert.SerializeObject(objectContent), Encoding.UTF8, "application/json");
                var dbServerResult = await _httpClient.PostAsync(_config["FinalizeEndpoint"], httpContent);
                // Throw exception if not success
                if (!dbServerResult.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }

    public class MyEventGridEvent
    {
        public string Id { get; set; }
        public string Topic { get; set; }
        public string Subject { get; set; }
        public string EventType { get; set; }
        public DateTime EventTime { get; set; }
        public EventGridEventData Data { get; set; }
    }

    public class EventGridEventData
    {
        public string api { get; set; }
        public string clientRequestId { get; set; }
        public string requestId { get; set; }
        public string eTag { get; set; }
        public string contentType { get; set; }
        public int contentLength { get; set; }
        public string blobType { get; set; }
        public string url { get; set; }
        public string sequencer { get; set; }
    }
}
