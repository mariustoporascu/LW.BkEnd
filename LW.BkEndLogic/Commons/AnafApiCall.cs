using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndLogic.Commons
{
    public interface IAnafApiCall
    {
        Task<FirmaAnafDetails?> CheckCui(int cui);
    }

    public class AnafApiCall : IAnafApiCall
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AnafApiCall> _logger;

        public AnafApiCall(IConfiguration configuration, ILogger<AnafApiCall> logger)
        {
            _httpClient = new HttpClient();
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<FirmaAnafDetails?> CheckCui(int cui)
        {
            var date = DateTime.UtcNow.AddHours(3);
            var dataAccAnaf =
                $"{date.Year}-{(date.Month > 9 ? date.Month : $"0{date.Month}")}-{(date.Day > 9 ? date.Day : $"0{date.Day}")}";

            var finalString = new StringContent(
                $"[{{\"cui\":{cui},\"data\":\"{dataAccAnaf}\"}}]",
                Encoding.UTF8,
                "application/json"
            );

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                _configuration["Anaf:ApiEndpoint"]
            );
            request.Content = finalString;

            var result = await _httpClient.SendAsync(request);
            _logger.LogInformation($"Anaf call result: {await result.Content.ReadAsStringAsync()}");
            if (result.IsSuccessStatusCode)
            {
                var response = JsonConvert.DeserializeObject<AnafResponse>(
                    await result.Content.ReadAsStringAsync()
                );
                if (response.Cod == 200 && response.Found.Count() > 0)
                {
                    return response.Found.First();
                }
            }
            return null;
        }
    }

    public class AnafResponse
    {
        [JsonProperty("cod")]
        public int Cod { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("found")]
        public IEnumerable<FirmaAnafDetails> Found { get; set; }
    }

    public class FirmaAnafDetails
    {
        [JsonProperty("adresa")]
        public string Adresa { get; set; }

        [JsonProperty("cui")]
        public string Cui { get; set; }

        [JsonProperty("scpTVA")]
        public bool ScpTVA { get; set; }

        [JsonProperty("nrRegCom")]
        public string NrRegCom { get; set; }

        [JsonProperty("denumire")]
        public string Denumire { get; set; }
    }
}
