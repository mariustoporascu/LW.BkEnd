﻿using LW.BkEndLogic.Commons;
using LW.BkEndLogic.Commons.Interfaces;
using LW.BkEndModel;
using LW.BkEndModel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using LW.BkEndLogic.MasterUser;
using LW.BkEndApi.Models;

namespace LW.BkEndApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Policy = "master-admin")]
    public class MasterAdminController : ControllerBase
    {
        private readonly ILogger<MasterAdminController> _logger;
        private readonly IDbRepoCommon _dbRepoCommon;
        private readonly IDbRepoMaster _dbRepoMaster;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IAnafApiCall _anafApiCall;

        public MasterAdminController(
            ILogger<MasterAdminController> logger,
            RoleManager<Role> roleManager,
            IDbRepoCommon dbRepoCommon,
            UserManager<User> userManager,
            IDbRepoMaster dbRepoMaster,
            IAnafApiCall anafApiCall
        )
        {
            _logger = logger;
            _dbRepoCommon = dbRepoCommon;
            _roleManager = roleManager;
            _userManager = userManager;
            _dbRepoMaster = dbRepoMaster;
            _anafApiCall = anafApiCall;
        }

        [HttpPut("create-role")]
        public async Task<IActionResult> CreateRole(int roleInt)
        {
            try
            {
                var result = await _roleManager.CreateAsync(
                    new Role { Name = Enum.GetName(typeof(RolesEnum), roleInt) }
                );
                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
            }

            return BadRequest();
        }

        [HttpDelete("delete-role")]
        public async Task<IActionResult> DeleteRole([FromQuery] int roleInt)
        {
            try
            {
                var result = await _roleManager.DeleteAsync(
                    await _roleManager.FindByNameAsync(Enum.GetName(typeof(RolesEnum), roleInt))
                );
                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role");
            }

            return BadRequest();
        }

        [HttpPut("add-user-to-role")]
        public async Task<IActionResult> AddUserToRole(string userId, int roleInt)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                var result = await _userManager.AddToRoleAsync(
                    user,
                    Enum.GetName(typeof(RolesEnum), roleInt)
                );
                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user to role");
            }

            return BadRequest();
        }

        [HttpDelete("remove-user-from-role")]
        public async Task<IActionResult> RemoveUserFromRole(
            [FromQuery] string userId,
            [FromQuery] int roleInt
        )
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                var result = await _userManager.RemoveFromRoleAsync(
                    user,
                    Enum.GetName(typeof(RolesEnum), roleInt)
                );
                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user from role");
            }

            return BadRequest();
        }

        [HttpGet("getDashboardData")]
        public IActionResult GetDashboardData()
        {
            object data = _dbRepoMaster.GetDashboardInfo();

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

        [HttpGet("getFirme")]
        public IActionResult GetFirme()
        {
            object data = _dbRepoMaster.GetFirmeDiscountList();

            if (data == null)
            {
                return NoContent();
            }

            return Ok(JsonConvert.SerializeObject(data));
        }

        [HttpGet("getFirmaExtendedDetails")]
        public async Task<IActionResult> GetFirme([FromQuery] Guid firmaId)
        {
            var data = await _dbRepoCommon.GetCommonEntity<FirmaDiscount>(firmaId);

            if (data == null)
            {
                return NoContent();
            }

            return Ok(JsonConvert.SerializeObject(data));
        }

        [HttpGet("getDocumente")]
        public IActionResult GetDocumente()
        {
            object data = _dbRepoMaster.GetDocumenteList();

            if (data == null)
            {
                return NoContent();
            }

            return Ok(JsonConvert.SerializeObject(data));
        }

        [HttpGet("getDocumentePreApproval")]
        public IActionResult GetDocumentePreApproval()
        {
            object data = _dbRepoMaster.GetDocumentePreAprobareList();

            if (data == null)
            {
                return NoContent();
            }

            return Ok(JsonConvert.SerializeObject(data));
        }

        [HttpPut("changeDocStatus")]
        public async Task<IActionResult> ChangeDocStatus(Guid documentId, StatusEnum status)
        {
            if ((int)status < 2 && (int)status > 3)
            {
                return BadRequest(new { Message = "Invalid status", Error = true });
            }
            var result = await _dbRepoMaster.ChangeDocStatus(documentId, status);
            if (result == false)
            {
                return BadRequest(
                    new { Message = "Document status failed to be updated", Error = true }
                );
            }
            return Ok(new { Message = "Document status has been updated", Error = false });
        }

        [HttpPost("addFirma")]
        public async Task<IActionResult> AddFirma([FromBody] CreateFirmaDiscountDTO firma)
        {
            if (
                string.IsNullOrWhiteSpace(firma.CuiNumber)
                || _dbRepoMaster.FirmaDiscountExists(firma.CuiNumber)
            )
            {
                return BadRequest(
                    new { Message = "Firma with same CUI already exists", Error = true }
                );
            }
            var firmaDiscount = new FirmaDiscount
            {
                Address = firma.Address,
                BankAccount = firma.BankAccount,
                BankName = firma.BankName,
                CuiNumber = firma.CuiNumber,
                MainContactEmail = firma.MainContactEmail,
                MainContactName = firma.MainContactName,
                MainContactPhone = firma.MainContactPhone,
                DiscountPercent = firma.DiscountPercent,
                NrRegCom = firma.NrRegCom,
                Name = firma.Name,
                NameAnaf = firma.NameAnaf,
            };
            var result = await _dbRepoCommon.AddCommonEntity(firmaDiscount);
            if (result == false)
            {
                return BadRequest(new { Message = "Firma failed to be added", Error = true });
            }
            return Ok(new { Message = "Firma has been added", Error = false });
        }

        [HttpPut("updateFirmaStatus")]
        public async Task<IActionResult> UpdateFirmaStatus(Guid firmaId, bool isSecondary)
        {
            var firmaDiscount = await _dbRepoCommon.GetCommonEntity<FirmaDiscount>(firmaId);
            if (firmaDiscount == null)
            {
                return BadRequest(new { Message = "Firma not found", Error = true });
            }
            if (isSecondary)
            {
                firmaDiscount.IsActiveSecondary = !firmaDiscount.IsActiveSecondary;
            }
            else
            {
                firmaDiscount.IsActive = !firmaDiscount.IsActive;
            }
            var result = await _dbRepoCommon.UpdateCommonEntity(firmaDiscount);
            if (result == false)
            {
                return BadRequest(
                    new { Message = "Firma status failed to be updated", Error = true }
                );
            }
            return Ok(new { Message = "Firma status has been updated", Error = false });
        }

        [HttpPost("updateFirma")]
        public async Task<IActionResult> UpdateFirma([FromBody] UpdateFirmaDiscountDTO firma)
        {
            var firmaDiscount = await _dbRepoCommon.GetCommonEntity<FirmaDiscount>(firma.Id);
            if (string.IsNullOrWhiteSpace(firma.CuiNumber) || firmaDiscount == null)
            {
                return BadRequest(new { Message = "Firma not found", Error = true });
            }
            if (
                firma.CuiNumber != firmaDiscount.CuiNumber
                && _dbRepoMaster.FirmaDiscountExists(firma.CuiNumber)
            )
            {
                return BadRequest(
                    new { Message = "Firma with same CUI already exists", Error = true }
                );
            }
            firmaDiscount.Address = firma.Address;
            firmaDiscount.BankAccount = firma.BankAccount;
            firmaDiscount.BankName = firma.BankName;
            firmaDiscount.CuiNumber = firma.CuiNumber;
            firmaDiscount.MainContactEmail = firma.MainContactEmail;
            firmaDiscount.MainContactName = firma.MainContactName;
            firmaDiscount.MainContactPhone = firma.MainContactPhone;
            firmaDiscount.DiscountPercent = firma.DiscountPercent;
            firmaDiscount.NrRegCom = firma.NrRegCom;
            firmaDiscount.Name = firma.Name;
            firmaDiscount.NameAnaf = firma.NameAnaf;
            var result = await _dbRepoCommon.UpdateCommonEntity(firmaDiscount);
            if (result == false)
            {
                return BadRequest(new { Message = "Firma failed to be updated", Error = true });
            }
            return Ok(new { Message = "Firma has been updated", Error = false });
        }

        [HttpGet("get-details-anaf")]
        public async Task<IActionResult> QueryUser([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return NoContent();
                }
                var result = await _anafApiCall.CheckCui(
                    int.Parse(query.ToLower().Replace("ro", ""))
                );
                if (result == null)
                {
                    return NoContent();
                }
                return Ok(JsonConvert.SerializeObject(result));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return NoContent();
            }
        }
    }
}
