using Castle.Core.Logging;
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
        private readonly IAnafApiCall _anafApiCall;

        public Anaf_CheckCui(ITestOutputHelper outputHelper, IAnafApiCall anafApiCall)
        {
            _outputHelper = outputHelper;
            _anafApiCall = anafApiCall;
        }

        [Fact]
        public void CheckRandomCuiResult()
        {
            var cui = "45329241";
            var result = _anafApiCall.CheckCui(int.Parse(cui)).GetAwaiter().GetResult();
            _outputHelper.WriteLine(JsonConvert.SerializeObject(result));
            result.Should().NotBeNull();
        }
    }
}
