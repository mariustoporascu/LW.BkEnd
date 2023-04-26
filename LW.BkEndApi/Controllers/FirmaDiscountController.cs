using LW.BkEndLogic.Commons;
using LW.BkEndLogic.FirmaDiscUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using LW.BkEndLogic.Commons.Interfaces;
using LW.BkEndApi.Models;
using Newtonsoft.Json.Serialization;

namespace LW.BkEndApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
	[Authorize]
	public class FirmaDiscountController : ControllerBase
	{
		private readonly IDbRepoFirma _dbRepoFirma;
		private readonly IDbRepoCommon _dbRepoCommon;
		public FirmaDiscountController(IDbRepoCommon dbRepoCommon, IDbRepoFirma dbRepoFirma)
		{
			_dbRepoCommon = dbRepoCommon;
			_dbRepoFirma = dbRepoFirma;
		}
		[HttpGet("getAllDocumenteWFP")]
		public IActionResult GetAllDocumente()
		{
			var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);

			var documents = _dbRepoFirma.GetAllDocumenteWFP(conexId);

			if (documents == null || documents.Count() == 0)
			{
				return NoContent();
			}

			return Ok(JsonConvert.SerializeObject(documents, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			}));
		}
		[HttpPost("updateDocStatus")]
		public async Task<IActionResult> UpdateDocStatus([FromBody] DocStatusUpdateModel docStatusUpdateModel)
		{
			List<bool> bools = new List<bool>();
			foreach (var id in docStatusUpdateModel.DocumenteIds)
			{
				var document = _dbRepoFirma.GetDocument(id);
				if (document == null || document.Status != 3)
				{
					bools.Add(false);
					continue;
				}
				var result = await _dbRepoFirma.UpdateDocStatusAsync(document, docStatusUpdateModel.Status);
				bools.Add(result);
			}
			if (bools.Any(b => b == false))
			{
				return BadRequest(new
				{
					Message = JsonConvert.SerializeObject(new
					{
						Succes = bools.Where(b => b == true).Count(),
						Failed = bools.Where(b => b == false).Count()
					}),
					Error = true
				});
			}
			return Ok(new { Message = "All tranzactions were succesfully completed", Error = false });
		}
		[HttpGet("getDashboardData")]
		public IActionResult GetDashboardData()
		{
			var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);

			var data = _dbRepoFirma.GetDashboardInfo(conexId);

			if (data == null)
			{
				return NoContent();
			}

			return Ok(JsonConvert.SerializeObject(data, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			}));
		}
	}
}
