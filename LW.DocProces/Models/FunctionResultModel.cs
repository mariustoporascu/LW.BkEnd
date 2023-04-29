using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace LW.DocProces.Models
{
	public class FunctionResultModel
	{
		public string BlobName { get; set; }
		public string AnalyzeResult { get; set; }
	}
}
