using Azure.AI.FormRecognizer.DocumentAnalysis;
using LW.BkEndModel;
using LW.BkEndModel.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LW.DocProcLogic.ProcessOcrResult
{
	public static class ResultProcessor
	{
		public static JObject ProcessInvoiceForFunctionApp(AnalyzeResult result)
		{
			JObject returnValue = new();
			return new();
		}
		public static JObject ProcessReceiptForFunctionApp(AnalyzeResult result)
		{
			JObject returnValue = new();

			var document = result.Documents.First();
			document.Fields.TryGetValue("MerchantName", out DocumentField MerchantName);
			document.Fields.TryGetValue("MerchantAddress", out DocumentField MerchantAddress);
			document.Fields.TryGetValue("TransactionDate", out DocumentField TransactionDate);
			document.Fields.TryGetValue("TransactionTime", out DocumentField TransactionTime);
			document.Fields.TryGetValue("Total", out DocumentField Total);
			document.Fields.TryGetValue("TotalTax", out DocumentField TotalTax);
			returnValue["Content"] = result.Content;
			returnValue["MerchantName"] = MerchantName?.Content;
			returnValue["MerchantAddress"] = MerchantAddress?.Content;
			try
			{
				returnValue["TransactionDate"] = TransactionDate?.Value.AsDate();

			}
			catch (Exception)
			{
				returnValue["TransactionDate"] = TransactionDate?.Content;
			}
			returnValue["TransactionTime"] = TransactionTime?.Content;
			try
			{
				returnValue["Total"] = Total?.Value.AsDouble();
			}
			catch (Exception)
			{
				returnValue["Total"] = Total?.Content;
			}
			try
			{
				returnValue["TotalTax"] = TotalTax?.Value.AsDouble();
			}
			catch (Exception)
			{
				returnValue["TotalTax"] = TotalTax?.Content;
			}
			return returnValue;
		}
		public static void ProcessReceiptForFileManager(ref Documente dbFile, FirmaDiscount dbFirmaDisc, JObject processedResult)
		{
			try
			{
				var docNumber = JsonConvert.DeserializeObject<JObject>(processedResult["docNumber"]?.ToString());
				dbFile.DocNumber = docNumber["data"]?.ToString();
			}
			catch (Exception)
			{
				dbFile.DocNumber = "";
			}
			dbFile.Total = decimal.Parse(processedResult["Total"]?.ToString() ?? "0.00");
			dbFile.DiscountValue = dbFile.Total * dbFirmaDisc.DiscountPercent / 100;
			dbFile.Status = (int)StatusEnum.CompletedProcessing;
			dbFile.StatusName = StatusEnum.CompletedProcessing.ToString();
			dbFile.ExtractedBusinessAddress = processedResult["MerchantAddress"]?.ToString();
			dbFile.ExtractedBusinessData = processedResult["MerchantName"]?.ToString();
		}
	}
}
