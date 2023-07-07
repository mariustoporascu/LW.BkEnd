using LW.BkEndLogic.Commons;
using LW.BkEndLogic.Commons.Interfaces;
using LW.BkEndModel;
using LW.BkEndModel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace LW.BkEndApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Policy = "master-admin")]
    public class MasterAdminController : ControllerBase
    {
        private readonly ILogger<MasterAdminController> _logger;
        private readonly IDbRepoCommon _dbRepoCommon;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;

        public MasterAdminController(
            ILogger<MasterAdminController> logger,
            RoleManager<Role> roleManager,
            IDbRepoCommon dbRepoCommon,
            UserManager<User> userManager
        )
        {
            _logger = logger;
            _dbRepoCommon = dbRepoCommon;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet("create-role")]
        public async Task<IActionResult> CreateRole([FromQuery] int roleInt)
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

        [HttpGet("add-user-to-role")]
        public async Task<IActionResult> AddUserToRole(
            [FromQuery] string userId,
            [FromQuery] int roleInt
        )
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
            var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);

            object data = null; //_dbRepoUser.GetDashboardInfo(conexId);

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
