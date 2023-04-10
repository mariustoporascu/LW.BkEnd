using LW.BkEndLogic.Commons;
using LW.BkEndLogic.RegularUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LW.BkEndApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
	//[Authorize]
	public class RegularUserController : ControllerBase
	{
		private readonly IDbRepoUser _dbRepoUser;
		private readonly IDbRepoCommon _dbRepoCommon;
		public RegularUserController(IDbRepoUser dbRepoUser, IDbRepoCommon dbRepoCommon)
		{
			_dbRepoUser = dbRepoUser;
			_dbRepoCommon = dbRepoCommon;
		}
		[HttpGet("getAllDocumente")]
		public IActionResult GetAllDocumente([FromQuery(Name = "conexId")] Guid conexId)
		{
			if (conexId == Guid.Empty)
			{
				return BadRequest("Invalid input");
			}

			var documents = _dbRepoUser.GetAllDocumente(conexId).AsEnumerable();

			if (documents == null || documents.Count() == 0)
			{
				return NoContent();
			}

			return Ok(JsonConvert.SerializeObject(documents));
		}
		[HttpGet("getAllDataProc")]
		public IActionResult GetAllDataProc([FromQuery(Name = "conexId")] Guid conexId)
		{
			if (conexId == Guid.Empty)
			{
				return BadRequest("Invalid input");
			}

			var documents = _dbRepoUser.GetAllDataProcDocs(conexId).AsEnumerable();

			if (documents == null || documents.Count() == 0)
			{
				return NoContent();
			}

			return Ok(JsonConvert.SerializeObject(documents));
		}
	}
}
