﻿using LW.BkEndLogic.Commons;
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
    [Authorize(Policy = "firma-admin")]
    public class FirmaDiscountController : ControllerBase
    {
        private readonly IDbRepoFirma _dbRepoFirma;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<FirmaDiscountController> _logger;
        private readonly IDbRepoCommon _dbRepoCommon;
        private readonly IEmailSender _emailSender;

        public FirmaDiscountController(
            IDbRepoCommon dbRepoCommon,
            IDbRepoFirma dbRepoFirma,
            IEmailSender emailSender,
            SignInManager<User> signInManager,
            ILogger<FirmaDiscountController> logger
        )
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

            return Ok(
                JsonConvert.SerializeObject(
                    documents,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }
                )
            );
        }

        [HttpPost("updateDocStatus")]
        public async Task<IActionResult> UpdateDocStatus(
            [FromBody] DocStatusUpdateModel docStatusUpdateModel
        )
        {
            List<bool> bools = new List<bool>();
            foreach (var id in docStatusUpdateModel.DocumenteIds)
            {
                var document = await _dbRepoCommon.GetCommonEntity<Documente>(id);
                if (document == null || document.Status != 3)
                {
                    bools.Add(false);
                    continue;
                }
                var result = await _dbRepoFirma.UpdateDocStatusAsync(
                    document,
                    docStatusUpdateModel.Status
                );
                bools.Add(result);
            }
            if (bools.Any(b => b == false))
            {
                return BadRequest(
                    new
                    {
                        Message = new
                        {
                            Succes = bools.Where(b => b == true).Count(),
                            Failed = bools.Where(b => b == false).Count()
                        },
                        Error = true
                    }
                );
            }
            return Ok(
                new { Message = "All tranzactions were succesfully completed", Error = false }
            );
        }

        [HttpPut("updateHybrid")]
        public async Task<IActionResult> UpdateHybrid(string name, string id)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { Message = "Hybrid name cannot be empty", Error = true });
            }
            var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
            var firmaDiscountId = _dbRepoFirma.GetFirmaDiscountId(conexId);
            if (firmaDiscountId == Guid.Empty)
            {
                return BadRequest(new { Message = "Firma discount not found", Error = true });
            }
            var hybridGuid = new Guid(id);
            var hybrid = await _dbRepoCommon.GetCommonEntity<Hybrid>(hybridGuid);
            if (hybrid == null)
            {
                return BadRequest(new { Message = "Hybrid not found", Error = true });
            }
            if (
                name != hybrid.Name
                && await _dbRepoFirma.CheckIfHybrindExists(name, firmaDiscountId)
            )
            {
                return BadRequest(new { Message = "Hybrid name already exists", Error = true });
            }
            hybrid.Name = name;
            var result = await _dbRepoCommon.UpdateCommonEntity(hybrid);
            if (result)
            {
                return Ok(new { Message = "Hybrid updated succesfully", Error = false });
            }
            return BadRequest(new { Message = "Hybrid update failed", Error = true });
        }

        [HttpPost("createHybrid")]
        public async Task<IActionResult> CreateHybrid([FromBody] CreateHybridDTO createHybridDTO)
        {
            var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
            var firmaDiscountId = _dbRepoFirma.GetFirmaDiscountId(conexId);
            if (firmaDiscountId == Guid.Empty)
            {
                return BadRequest(new { Message = "Firma discount not found", Error = true });
            }
            if (string.IsNullOrWhiteSpace(createHybridDTO.Name))
            {
                return BadRequest(new { Message = "Hybrid name cannot be empty", Error = true });
            }
            if (await _dbRepoFirma.CheckIfHybrindExists(createHybridDTO.Name, firmaDiscountId))
            {
                return BadRequest(new { Message = "Hybrid name already exists", Error = true });
            }
            var hybrid = new Hybrid
            {
                Name = createHybridDTO.Name,
                FirmaDiscountId = firmaDiscountId,
            };
            var hybridResult = await _dbRepoCommon.AddCommonEntity(hybrid);
            if (!hybridResult)
            {
                return BadRequest(new { Message = "Hybrid could not get created", Error = true });
            }
            if (
                !string.IsNullOrWhiteSpace(createHybridDTO.InitialEmail)
                && !string.IsNullOrWhiteSpace(createHybridDTO.InitialPassword)
            )
            {
                var user = new User
                {
                    UserName = createHybridDTO.InitialEmail,
                    Email = createHybridDTO.InitialEmail,
                    EmailConfirmed = true,
                };
                var userResult = await _signInManager.UserManager.CreateAsync(
                    user,
                    createHybridDTO.InitialPassword
                );
                if (!userResult.Succeeded)
                {
                    await _dbRepoCommon.DeleteCommonEntity(hybrid);
                    return BadRequest(
                        new { Message = "User for hybrid could not get created", Error = true }
                    );
                }

                var conexUser = new ConexiuniConturi
                {
                    UserId = user.Id,
                    HybridId = hybrid.Id,
                    ProfilCont = new ProfilCont { Email = createHybridDTO.InitialEmail, }
                };
                var conexUserResult = await _dbRepoCommon.AddCommonEntity(conexUser);
                if (!conexUserResult)
                {
                    await _dbRepoCommon.DeleteCommonEntity(hybrid);
                    return BadRequest(
                        new { Message = "Conexiuni conturi could not get created", Error = true }
                    );
                }
                if (
                    !_emailSender.SendEmail(
                        new string[] { user.Email },
                        "Creare cont automat",
                        $"Contul tau a fost creat automat cu parola {createHybridDTO.InitialPassword}, te rugam sa o schimbi dupa prima autentificare."
                    )
                )
                {
                    _logger.LogWarning("Confirmation Email failed to be sent");
                }
                else
                {
                    _logger.LogInformation("Hybrid account created succesfully");
                }
            }
            return Ok(new { Message = "Hybrid created succesfully", Error = false });
        }

        [HttpGet("checkIfEmailNotTaken")]
        public IActionResult CheckIfEmailNotTaken(string email)
        {
            return Ok(new { Result = _dbRepoCommon.EmailNotTaken(email) });
        }

        [HttpDelete("deleteHybrids")]
        public async Task<IActionResult> DeleteHybrid(
            [FromBody] DeleteHybridsModel deleteHybridsModel
        )
        {
            var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
            var firmaDiscountId = _dbRepoFirma.GetFirmaDiscountId(conexId);
            List<bool> bools = new List<bool>();

            if (firmaDiscountId == Guid.Empty)
            {
                return BadRequest(new { Message = "Firma discount not found", Error = true });
            }
            foreach (var hybridId in deleteHybridsModel.GroupsIds)
            {
                bools.Add(await _dbRepoFirma.DeleteHybrid(firmaDiscountId, hybridId));
            }
            if (bools.Any(b => b == false))
            {
                return BadRequest(
                    new
                    {
                        Message = new
                        {
                            Succes = bools.Where(b => b == true).Count(),
                            Failed = bools.Where(b => b == false).Count()
                        },
                        Error = true
                    }
                );
            }
            return Ok(
                new { Message = "All tranzactions were succesfully completed", Error = false }
            );
        }

        [HttpGet("getFirmaHybrids")]
        public async Task<IActionResult> GetFirmaHybrids()
        {
            var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
            var firmaDiscountId = _dbRepoFirma.GetFirmaDiscountId(conexId);
            if (firmaDiscountId == Guid.Empty)
            {
                return BadRequest(new { Message = "Firma discount not found", Error = true });
            }
            var data = _dbRepoFirma.GetAllHybrids(firmaDiscountId);

            if (data == null)
            {
                return NoContent();
            }

            return Ok(
                JsonConvert.SerializeObject(
                    data,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }
                )
            );
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

            return Ok(
                JsonConvert.SerializeObject(
                    data,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }
                )
            );
        }
    }
}
