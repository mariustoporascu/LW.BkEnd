using Microsoft.Extensions.Configuration;
using System.Text;

namespace LW.DocProcLogic.Anaf
{
	public interface IAnafApiCall
	{
		Task<string> CheckCui(int cui);
	}
	public class AnafApiCall : IAnafApiCall
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		public AnafApiCall(HttpClient httpClient, IConfiguration configuration)
		{
			_httpClient = httpClient;
			_configuration = configuration;
		}
		public async Task<string> CheckCui(int cui)
		{
			var date = DateTime.UtcNow.AddHours(3);
			var dataAccAnaf = $"{date.Year}-{(date.Month > 9 ? date.Month : $"0{date.Month}")}-{(date.Day > 9 ? date.Day : $"0{date.Day}")}";

			var finalString = new StringContent($"[{{\"cui\":{cui},\"data\":\"{dataAccAnaf}\"}}]", Encoding.UTF8, "application/json");

			var request = new HttpRequestMessage(HttpMethod.Post, _configuration["Anaf:ApiEndpoint"]);
			request.Content = finalString;

			var result = await _httpClient.SendAsync(request);
			if (result.IsSuccessStatusCode)
			{
				return await result.Content.ReadAsStringAsync();
			}
			return string.Empty;
		}
	}
}
