using LW.BkEndLogic.Commons;
using LW.BkEndLogic.FirmaDiscUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using LW.BkEndLogic.Commons.Interfaces;
using LW.BkEndApi.Models;
using Newtonsoft.Json.Serialization;
using LW.BkEndModel;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;

namespace LW.BkEndApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
	[Authorize]
	public class FirmaDiscountController : ControllerBase
	{
		private readonly IDbRepoFirma _dbRepoFirma;
		private readonly SignInManager<User> _signInManager;
		private readonly ILogger<FirmaDiscountController> _logger;
		private readonly IDbRepoCommon _dbRepoCommon;
		private readonly IEmailSender _emailSender;

		public FirmaDiscountController(IDbRepoCommon dbRepoCommon,
			IDbRepoFirma dbRepoFirma, IEmailSender emailSender,
			SignInManager<User> signInManager, ILogger<FirmaDiscountController> logger)
		{
			_dbRepoCommon = dbRepoCommon;
			_dbRepoFirma = dbRepoFirma;
			_emailSender = emailSender;
			_signInManager = signInManager;
			_logger = logger;
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
		[HttpPost("createHybrid")]
		public async Task<IActionResult> CreateHybrid([FromBody] CreateHybridDTO createHybridDTO)
		{
			var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
			var firmaDiscountId = _dbRepoFirma.GetFirmaDiscountId(conexId);
			if (firmaDiscountId == null)
			{
				return BadRequest(new
				{
					Message = "Firma discount not found",
					Error = true
				});
			}
			if (!string.IsNullOrWhiteSpace(createHybridDTO.Name))
			{
				var hybrid = new Hybrid
				{
					Name = createHybridDTO.Name,
					FirmaDiscountId = firmaDiscountId.Value,
				};
				var hybridResult = await _dbRepoCommon.AddCommonEntity(hybrid);
				if (!hybridResult)
				{
					return BadRequest(new
					{
						Message = "Hybrid could not get created",
						Error = true
					});
				}
				if (!string.IsNullOrWhiteSpace(createHybridDTO.InitialEmail) &&
					!string.IsNullOrWhiteSpace(createHybridDTO.InitialPassword))
				{
					var user = new User
					{
						UserName = createHybridDTO.InitialEmail,
						Email = createHybridDTO.InitialEmail,
						EmailConfirmed = true,
					};
					var userResult = await _signInManager.UserManager.CreateAsync(user, createHybridDTO.InitialPassword);
					if (!userResult.Succeeded)
					{
						return BadRequest(new
						{
							Message = "User for hybrid could not get created",
							Error = true
						});
					}

					var conexUser = new ConexiuniConturi
					{
						UserId = user.Id,
						HybridId = hybrid.Id
					};
					var conexUserResult = await _dbRepoCommon.AddCommonEntity(conexUser);
					if (!conexUserResult)
					{
						return BadRequest(new
						{
							Message = "Conexiuni conturi could not get created",
							Error = true
						});
					}
					if (!_emailSender.SendEmail(new string[] { user.Email }, "Creare cont automat", $"Contul tau a fost creat automat cu parola {createHybridDTO.InitialPassword}, te rugam sa o schimbi dupa prima autentificare."))
					{
						_logger.LogWarning("Confirmation Email failed to be sent");
					}
					else
					{
						_logger.LogInformation("Hybrid account created succesfully");
					}
				}
			}
			return Ok(new { Message = "Hybrid created succesfully", Error = false });
		}
		[HttpGet("checkIfEmailNotTaken")]
		public IActionResult CheckIfEmailNotTaken(string email)
		{
			return Ok(new { Result = _dbRepoCommon.EmailNotTaken(email) });
		}
		[HttpPost("deleteHybrids")]
		public async Task<IActionResult> DeleteHybrid([FromBody] DeleteHybridsModel deleteHybridsModel)
		{
			var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
			var firmaDiscountId = _dbRepoFirma.GetFirmaDiscountId(conexId);
			List<bool> bools = new List<bool>();

			if (firmaDiscountId == null)
			{
				return BadRequest(new
				{
					Message = "Firma discount not found",
					Error = true
				});
			}
			foreach (var hybridId in deleteHybridsModel.GroupsIds)
			{
				bools.Add(await _dbRepoFirma.DeleteHybrid((Guid)firmaDiscountId, hybridId));
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
		[HttpGet("getFirmaHybrids")]
		public async Task<IActionResult> GetFirmaHybrids()
		{
			var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
			var firmaDiscountId = _dbRepoFirma.GetFirmaDiscountId(conexId);
			if (firmaDiscountId == null)
			{
				return BadRequest(new
				{
					Message = "Firma discount not found",
					Error = true
				});
			}
			var data = _dbRepoFirma.GetAllHybrids((Guid)firmaDiscountId);

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
