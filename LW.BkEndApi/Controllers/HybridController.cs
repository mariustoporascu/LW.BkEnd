using LW.BkEndApi.Models;
using LW.BkEndLogic.Commons.Interfaces;
using LW.BkEndLogic.FirmaDiscUser;
using LW.BkEndModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using LW.BkEndLogic.HybridUser;

namespace LW.BkEndApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
	[Authorize]
	public class HybridController : ControllerBase
	{
		private readonly IDbRepoHybrid _dbRepoHybrid;
		private readonly SignInManager<User> _signInManager;
		private readonly ILogger<HybridController> _logger;
		private readonly IDbRepoCommon _dbRepoCommon;
		private readonly IEmailSender _emailSender;

		public HybridController(IDbRepoCommon dbRepoCommon,
			IDbRepoHybrid dbRepoHybrid, IEmailSender emailSender,
			SignInManager<User> signInManager, ILogger<HybridController> logger)
		{
			_dbRepoCommon = dbRepoCommon;
			_dbRepoHybrid = dbRepoHybrid;
			_emailSender = emailSender;
			_signInManager = signInManager;
			_logger = logger;
		}
		[HttpGet("getAllDocumenteFileManager")]
		public IActionResult GetAllDocumenteFileManager()
		{
			var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
			var hybridId = _dbRepoHybrid.GetMyHybridId(conexId);

			var documents = _dbRepoHybrid.GetAllDocumenteFileManager(hybridId);

			if (documents == null || documents.Count() == 0)
			{
				return NoContent();
			}

			return Ok(JsonConvert.SerializeObject(documents));
		}
		[HttpGet("getAllDocumenteOperatii")]
		public IActionResult GetAllDocumenteOperatii()
		{
			var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
			var hybridId = _dbRepoHybrid.GetMyHybridId(conexId);

			var documents = _dbRepoHybrid.GetAllDocumenteOperatii(hybridId);

			if (documents == null || documents.Count() == 0)
			{
				return NoContent();
			}

			return Ok(JsonConvert.SerializeObject(documents));
		}

		[HttpGet("getAllFolders")]
		public IActionResult GetAllFolders()
		{
			var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
			var hybridId = _dbRepoHybrid.GetMyHybridId(conexId);

			var folders = _dbRepoCommon.GetAllFolders(hybridId);

			if (folders == null || folders.Count() == 0)
			{
				return NoContent();
			}

			return Ok(JsonConvert.SerializeObject(folders));
		}
		[HttpGet("getDashboardData")]
		public IActionResult GetDashboardData()
		{
			var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
			var hybridId = _dbRepoHybrid.GetMyHybridId(conexId);

			var data = _dbRepoHybrid.GetDashboardInfo(hybridId);

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
		[HttpGet("getAllTransfers")]
		public IActionResult GetAllTransfers()
		{
			var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
			var hybridId = _dbRepoHybrid.GetMyHybridId(conexId);

			var data = _dbRepoHybrid.GetAllTranzactiiTransfer(hybridId);

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

		[HttpPost("addTranzaction")]
		public async Task<IActionResult> AddTranzaction([FromBody] TranzactionModel tranzactionModel)
		{
			var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);

			List<bool> bools = new List<bool>();
			foreach (var id in tranzactionModel.DocumenteIds)
			{
				var document = _dbRepoHybrid.GetDocument(id);
				if (document == null || document.Status != 1)
				{
					bools.Add(false);
					continue;
				}
				var result = await _dbRepoHybrid.AddTranzaction(conexId, document, tranzactionModel.TranzactionType, tranzactionModel.NextConexId);
				bools.Add(result);
			}
			if (bools.Any(b => b == false))
			{
				return BadRequest(new
				{
					Message = new
					{
						Succes = bools.Where(b => b == true).Count(),
						Failed = bools.Where(b => b == false).Count()
					},
					Error = true
				});
			}
			return Ok(new { Message = "All tranzactions were succesfully completed", Error = false });
		}
		[HttpGet("query-users")]
		public IActionResult QueryUser([FromQuery] string query)
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				return NoContent();
			}
			var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
			var users = _dbRepoCommon.FindUsers(query);
			if (users == null || users.Count() == 0)
			{
				return NoContent();
			}
			return Ok(JsonConvert.SerializeObject(users));
		}
	}
}
