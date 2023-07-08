using LW.BkEndLogic.Commons;
using LW.BkEndLogic.RegularUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using LW.BkEndLogic.Commons.Interfaces;
using Newtonsoft.Json.Serialization;
using LW.BkEndModel.Enums;
using LW.BkEndApi.Models;
using LW.BkEndModel;

namespace LW.BkEndApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Policy = "user")]
    public class RegularUserController : ControllerBase
    {
        private readonly IDbRepoUser _dbRepoUser;
        private readonly IDbRepoCommon _dbRepoCommon;

        public RegularUserController(IDbRepoUser dbRepoUser, IDbRepoCommon dbRepoCommon)
        {
            _dbRepoUser = dbRepoUser;
            _dbRepoCommon = dbRepoCommon;
        }

        [HttpGet("getAllDocumenteFileManager")]
        public IActionResult GetAllDocumenteFileManager()
        {
            var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);

            var documents = _dbRepoUser.GetAllDocumenteFileManager(conexId);

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

            var documents = _dbRepoUser.GetAllDocumenteOperatii(conexId);

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
            var hybridId = _dbRepoUser.GetMyHybridId(conexId);
            IEnumerable<FirmaDiscount> folders;
            if (hybridId == Guid.Empty)
                folders = _dbRepoCommon.GetAllFolders();
            else
                folders = _dbRepoCommon.GetAllFolders(hybridId);
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

            var data = _dbRepoUser.GetDashboardInfo(conexId);

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

        [HttpGet("getAllTransfers")]
        public IActionResult GetAllTransfers()
        {
            var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);

            var data = _dbRepoUser.GetAllTranzactiiTransfer(conexId);

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

        [HttpGet("getAllWithdrawals")]
        public IActionResult GetAllWithdrawals()
        {
            var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);

            var data = _dbRepoUser.GetAllTranzactiiWithDraw(conexId);

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

        [HttpPost("addTranzaction")]
        public async Task<IActionResult> AddTranzaction(
            [FromBody] TranzactionModel tranzactionModel
        )
        {
            var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
            List<bool> bools = new List<bool>();
            foreach (var id in tranzactionModel.DocumenteIds)
            {
                var document = _dbRepoUser.GetDocument(id);
                if (document == null || document.Status != 1)
                {
                    bools.Add(false);
                    continue;
                }
                var result = await _dbRepoUser.AddTranzaction(
                    conexId,
                    document,
                    tranzactionModel.TranzactionType,
                    tranzactionModel.NextConexId
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

        [HttpGet("query-users")]
        public IActionResult QueryUser([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return NoContent();
            }
            var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
            var users = _dbRepoCommon.FindUsers(query, conexId);
            if (users == null || users.Count() == 0)
            {
                return NoContent();
            }
            return Ok(JsonConvert.SerializeObject(users));
        }

        [HttpGet("add-favorite-user")]
        public async Task<IActionResult> AddFavoriteUser([FromQuery] string favConexId)
        {
            if (string.IsNullOrWhiteSpace(favConexId))
            {
                return NoContent();
            }
            var favConexIdGuid = new Guid(favConexId);
            var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
            var result = await _dbRepoCommon.AddFavoriteUser(conexId, favConexIdGuid);
            if (result == false)
            {
                return BadRequest(
                    new { Message = "Failed to add user to favorites", Error = true }
                );
            }
            return Ok(new { Message = "User added to favorites", Error = false });
        }

        [HttpDelete("remove-favorite-user")]
        public async Task<IActionResult> RemoveFavoriteUser([FromQuery] string favConexId)
        {
            if (string.IsNullOrWhiteSpace(favConexId))
            {
                return NoContent();
            }
            var favConexIdGuid = new Guid(favConexId);
            var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
            var result = await _dbRepoCommon.RemoveFavoriteUser(conexId, favConexIdGuid);
            if (result == false)
            {
                return BadRequest(
                    new { Message = "Failed to remove user from favorites", Error = true }
                );
            }
            return Ok(new { Message = "User removed from favorites", Error = false });
        }

        [HttpGet("get-favorite-users")]
        public IActionResult GetFavoriteUsers()
        {
            var conexId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "conexId").Value);
            var users = _dbRepoCommon.GetFavoritesList(conexId);
            if (users == null || users.Count() == 0)
            {
                return NoContent();
            }
            return Ok(JsonConvert.SerializeObject(users));
        }
    }
}
