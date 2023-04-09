using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.DocProcLogic.MicrosoftOcr
{
	public interface IOcrPrebuilt
	{
		Task<AnalyzeResult> SendToOcrAsync(Stream fileStream, string ocrModel);
	}

	public class OcrPrebuilt : IOcrPrebuilt
	{
		private readonly IConfiguration _configuration;
		public OcrPrebuilt(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		/*
		 * prebuilt-receipt
		 * prebuilt-invoice
		 */
		public async Task<AnalyzeResult> SendToOcrAsync(Stream fileStream, string ocrModel)
		{
			AzureKeyCredential credential = new AzureKeyCredential(_configuration["FormRecognizer:Key"]);
			DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(_configuration["FormRecognizer:Endpoint"]), credential);

			AnalyzeDocumentOperation operation = await client
				.AnalyzeDocumentAsync(WaitUntil.Completed, ocrModel, fileStream, new AnalyzeDocumentOptions
				{
					Locale = "es",
				});

			return operation.Value;
		}
	}
}
