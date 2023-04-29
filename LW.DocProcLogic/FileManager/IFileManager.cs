using Azure.Storage.Blobs.Models;
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
using LW.DocProcLogic.ExtractBarCode;
using LW.DocProcLogic.ProcessOcrResult;
using Azure.AI.FormRecognizer.Models;

namespace LW.DocProcLogic.FileManager
{
	public interface IFileManager
	{
		Task<bool> OnFileUpload(FormFile formFile, Guid conexId, Guid firmaDiscId);
		Task<bool> OnFileProcessed(string blobName, string result);
		Task<Stream> GetFileStream(string identifier, Guid conexId);
	}
	public class FileManager : IFileManager
	{
		private readonly IConfiguration _config;
		private readonly IDbRepo _dbRepo;
		public FileManager(IConfiguration config, IDbRepo dbRepo)
		{
			_config = config;
			_dbRepo = dbRepo;
		}

		public async Task<Stream> GetFileStream(string identifier, Guid conexId)
		{
			var exists = _dbRepo.GetDocumentByBlobName(identifier);
			if (exists == null || exists.ConexId != conexId)
			{
				return null;
			}
			var blobClient = new BlobClient(_config["Azure:Storage"], _config["Azure:ContainerName"], identifier);
			var response = await blobClient.DownloadAsync();

			return response?.Value?.Content;
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
					ResultProcessor.ProcessReceiptForFileManager(ref dbFile, dbFirmaDisc, processedResult);
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
				Console.WriteLine(ex.Message);
			}

			return await _dbRepo.UpdateCommonEntity(dbFile);
		}

		public async Task<bool> OnFileUpload(FormFile formFile, Guid conexId, Guid firmaDiscId)
		{
			try
			{
				using Stream stream = formFile.OpenReadStream();

				//var docNumber = ExtractBarcode.GetFromImage(stream);
				//if (string.IsNullOrEmpty(docNumber))
				//{
				//	return false;
				//}
				var dbFile = new Documente
				{
					ConexId = conexId,
					FirmaDiscountId = firmaDiscId,
					FisiereDocumente = new FisiereDocumente
					{
						FileName = formFile.FileName,
						FileExtension = formFile.ContentType,
					}
				};
				if (!await _dbRepo.AddCommonEntity(dbFile))
				{
					return false;
				}
				// Create a BlobServiceClient object using the connection string
				BlobServiceClient blobServiceClient = new BlobServiceClient(_config["Azure:Storage"]);

				// Get a reference to the blob container
				BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(_config["Azure:ContainerName"]);

				// Get a reference to the blob
				BlobClient blobClient = blobContainerClient.GetBlobClient(dbFile.FisiereDocumente.Identifier);
				stream.Seek(0, SeekOrigin.Begin);
				// Upload the file to the blob
				await blobClient.UploadAsync(stream, new BlobUploadOptions
				{
					HttpHeaders = new BlobHttpHeaders
					{
						ContentType = formFile.ContentType
					}
				});

				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				// Handle exceptions
				return false;
			}
		}
	}
}
