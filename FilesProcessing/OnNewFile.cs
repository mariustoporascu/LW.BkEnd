using System;
using System.IO;
using System.Text;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Storage.Blobs.Models;
using LW.BkEndModel.Enums;
using LW.DocProcLogic.DbRepo;
using LW.DocProcLogic.MicrosoftOcr;
using LW.DocProcLogic.ProcessOcrResult;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace FilesProcessing
{
	public class OnNewFile
	{
		private readonly ILogger _logger;
		private readonly HttpClient _httpClient;
		private readonly IOcrPrebuilt _ocrPrebuilt;
		private readonly IConfiguration _config;
		private readonly IDbRepo _dbRepo;

		public OnNewFile(ILoggerFactory loggerFactory, IOcrPrebuilt ocrPrebuilt, IConfiguration config, IDbRepo dbRepo)
		{
			_logger = loggerFactory.CreateLogger<OnNewFile>();
			_httpClient = new HttpClient();
			_ocrPrebuilt = ocrPrebuilt;
			_config = config;
			_dbRepo = dbRepo;
		}

		[Function("OnNewFile")]
		public async Task Run([BlobTrigger("uploadsblob/{name}", Connection = "AzureWebJobsStorage")] ReadOnlyMemory<byte> myBlob, string name)
		{
			_logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name}");
			// Convert ReadOnlyMemory<byte> to Stream
			using var stream = new MemoryStream(myBlob.ToArray());
			// Update status to Processing && get if invoice or receipt
			await _dbRepo.UpdateBlobStatus(name, StatusEnum.Processing);
			var blobType = _dbRepo.GetBlobType(name);

			if (!blobType)
			{
				// get document number from barcode
				var imageContent = new StreamContent(stream);
				var barCodeResult = await _httpClient.PostAsync(_config["BarCodeEndpoint"], imageContent);

				var barCode = await barCodeResult.Content.ReadAsStringAsync();
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
				// get document number from barcode
				var imageContent = new StreamContent(stream);

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
}
