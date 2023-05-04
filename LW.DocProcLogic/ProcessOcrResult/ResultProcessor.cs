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
			var ocrObject = new JObject();
			bool hasDocNumberErrors = false;
			List<bool> otherErrors = new List<bool>();
			if (!string.IsNullOrWhiteSpace(processedResult["docNumber"]?.ToString()))
			{
				var docNumber = JsonConvert.DeserializeObject<JObject>(processedResult["docNumber"]?.ToString());
				ocrObject["docNumber"] = JsonConvert.SerializeObject(new
				{
					value = docNumber["data"]?.ToString(),
					hasErrors = false
				});
			}
			else
			{
				ocrObject["docNumber"] = JsonConvert.SerializeObject(new
				{
					value = "",
					hasErrors = true,
					errorMessage = "Cod de bare/QR negasit"
				});
				hasDocNumberErrors = true;
			}

			if (!string.IsNullOrWhiteSpace(processedResult["Total"]?.ToString()))
			{
				decimal total = decimal.Parse(processedResult["Total"]?.ToString());
				ocrObject["total"] = JsonConvert.SerializeObject(new
				{
					value = total,
					hasErrors = false
				});
				dbFile.DiscountValue = total * dbFirmaDisc.DiscountPercent / 100;

			}
			else
			{
				ocrObject["total"] = JsonConvert.SerializeObject(new
				{
					value = 0,
					hasErrors = true,
					errorMessage = "Total negasit"
				});
			}
			if (processedResult["Content"].ToString().Contains(dbFirmaDisc.CuiNumber))
			{
				ocrObject["cuiFirma"] = JsonConvert.SerializeObject(new
				{
					value = dbFirmaDisc.CuiNumber,
					hasErrors = false
				});
				otherErrors.Add(false);
			}
			else
			{
				ocrObject["cuiFirma"] = JsonConvert.SerializeObject(new
				{
					value = "",
					hasErrors = true,
					errorMessage = "CUI negasit"
				});
				otherErrors.Add(true);
			}
			if (!string.IsNullOrWhiteSpace(processedResult["MerchantName"].ToString()))
			{
				ocrObject["denumireFirma"] = JsonConvert.SerializeObject(new
				{
					value = processedResult["MerchantName"].ToString(),
					hasErrors = false
				});
				otherErrors.Add(false);
			}
			else
			{
				ocrObject["denumireFirma"] = JsonConvert.SerializeObject(new
				{
					value = "",
					hasErrors = true,
					errorMessage = "Denumire firma negasita"
				});
				otherErrors.Add(true);
			}
			if (!string.IsNullOrWhiteSpace(processedResult["MerchantAddress"].ToString()))
			{
				ocrObject["adresaFirma"] = JsonConvert.SerializeObject(new
				{
					value = processedResult["MerchantAddress"].ToString(),
					hasErrors = false
				});
				otherErrors.Add(false);
			}
			else
			{
				ocrObject["adresaFirma"] = JsonConvert.SerializeObject(new
				{
					value = "",
					hasErrors = true,
					errorMessage = "Adresa firma negasita"
				});
				otherErrors.Add(true);
			}
			if (!string.IsNullOrWhiteSpace(processedResult["TransactionDate"].ToString()))
			{
				ocrObject["dataTranzactie"] = JsonConvert.SerializeObject(new
				{
					value = processedResult["TransactionDate"].ToString(),
					hasErrors = false
				});
				otherErrors.Add(false);
			}
			else
			{
				ocrObject["dataTranzactie"] = JsonConvert.SerializeObject(new
				{
					value = "",
					hasErrors = true,
					errorMessage = "Data tranzactie negasita"
				});
				otherErrors.Add(true);
			}
			if (!string.IsNullOrWhiteSpace(processedResult["TransactionTime"].ToString()))
			{
				ocrObject["oraTranzactie"] = JsonConvert.SerializeObject(new
				{
					value = processedResult["TransactionTime"].ToString(),
					hasErrors = false
				});
			}
			else
			{
				ocrObject["oraTranzactie"] = JsonConvert.SerializeObject(new
				{
					value = "",
					hasErrors = true,
					errorMessage = "Ora tranzactie negasita"
				});
			}
			if (!string.IsNullOrWhiteSpace(processedResult["TotalTax"].ToString()))
			{
				decimal totalTax = decimal.Parse(processedResult["TotalTax"].ToString());
				ocrObject["totalTva"] = JsonConvert.SerializeObject(new
				{
					value = totalTax,
					hasErrors = false
				});
			}
			else
			{
				ocrObject["totalTva"] = JsonConvert.SerializeObject(new
				{
					value = "",
					hasErrors = true,
					errorMessage = "Total TVA negasit"
				});
			}
			if (!hasDocNumberErrors && !otherErrors.Any(x => x))
			{
				dbFile.Status = (int)StatusEnum.WaitingForApproval;
				dbFile.StatusName = StatusEnum.WaitingForApproval.ToString();
			}
			else if (hasDocNumberErrors && !otherErrors.Any(x => x))
			{
				dbFile.Status = (int)StatusEnum.PartialyProcessed;
				dbFile.StatusName = StatusEnum.PartialyProcessed.ToString();
			}
			else
			{
				dbFile.Status = (int)StatusEnum.FailedProcessing;
				dbFile.StatusName = StatusEnum.FailedProcessing.ToString();
			}

		}
	}
}
