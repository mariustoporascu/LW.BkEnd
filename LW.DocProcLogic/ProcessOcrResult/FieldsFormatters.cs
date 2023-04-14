using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LW.DocProcLogic.ProcessOcrResult
{
	public static class FieldsFormatters
	{
		public static string ParseDecimalString(string value)
		{
			try
			{
				string result = string.Empty;
				foreach (var q in value)
				{
					if (Regex.IsMatch(q.ToString(), @"^[0-9\.\,]$"))
						result += q.ToString();
				}
				return result;
			}
			catch (Exception) { return string.Empty; }
		}
		public static string FormatDiacritics(string input)
		{
			try
			{
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

				byte[] tempBytes;
				tempBytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(input);
				string asciiStr = Encoding.UTF8.GetString(tempBytes);
				return asciiStr;
			}
			catch (Exception)
			{
				return string.Empty;
			}

		}
		public static decimal FormatStringDecimals(string str)
		{
			if (str.Contains(',') && str.Contains("."))
				if (str.IndexOf(',') < str.IndexOf('.'))
					str = str.Replace(",", "");
				else
					str = str.Replace(".", "").Replace(',', '.');
			else if (str.Contains(',') && !str.Contains('.'))
				str = str.Replace(',', '.');

			decimal.TryParse(str, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out decimal result);
			return result;
		}
		public static string FormatForExport(string format, decimal val) => string.Format(format, val);
	}
}
