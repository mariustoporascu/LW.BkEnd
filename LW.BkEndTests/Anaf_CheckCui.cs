using FluentAssertions;
using LW.BkEndLogic.Commons;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace LW.BkEndTests
{
    public class Anaf_CheckCui
    {
        public static string anafApi = "https://webservicesp.anaf.ro/PlatitorTvaRest/api/v6/ws/tva";
        private readonly ITestOutputHelper _outputHelper;

        public Anaf_CheckCui(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void CheckRandomCuiResult()
        {
            var cui = "45329241";
            var result = new AnafApiCall(
                new ConfigurationBuilder().AddJsonFile("appsettings.Production.json").Build()
            )
                .CheckCui(int.Parse(cui))
                .GetAwaiter()
                .GetResult();
            _outputHelper.WriteLine(JsonConvert.SerializeObject(result));
            result.Should().NotBeNull();
        }
    }
}
