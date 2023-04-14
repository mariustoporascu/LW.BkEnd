using LW.BkEndLogic.Commons;
using LW.BkEndLogic.FirmaDiscUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using LW.BkEndLogic.Commons.Interfaces;

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
		[HttpGet("getAllDocumente")]
		public IActionResult GetAllDocumente([FromQuery(Name = "firmaId")] Guid firmaId)
		{
			if (firmaId == Guid.Empty)
			{
				return BadRequest("Invalid input");
			}

			var documents = _dbRepoFirma.GetAllDocumente(firmaId);

			if (documents == null || documents.Count() == 0)
			{
				return NoContent();
			}

			return Ok(JsonConvert.SerializeObject(documents));
		}
	}
}
