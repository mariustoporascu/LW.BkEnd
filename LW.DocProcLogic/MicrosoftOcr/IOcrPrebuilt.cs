using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Extensions.Configuration;

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
			AzureKeyCredential credential = new AzureKeyCredential(_configuration["FormRecognizerKey"]);
			DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(_configuration["FormRecognizerEndpoint"]), credential);

			AnalyzeDocumentOperation operation = await client
				.AnalyzeDocumentAsync(WaitUntil.Completed, ocrModel, fileStream, new AnalyzeDocumentOptions
				{
					Locale = ocrModel.Equals("prebuilt-invoice") ? "es" : "es-ES",
				});

			return operation.Value;
		}
	}
}
