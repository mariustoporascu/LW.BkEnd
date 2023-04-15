﻿using LW.BkEndLogic.Commons;
using LW.BkEndLogic.RegularUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using LW.BkEndLogic.Commons.Interfaces;
using Newtonsoft.Json.Serialization;

namespace LW.BkEndApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
	[Authorize]
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
		public IActionResult GetAllDocumente()
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
		public IActionResult GetAllDocumenteApproved()
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
			var folders = _dbRepoCommon.GetAllFolders();

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

			return Ok(JsonConvert.SerializeObject(data, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			}));
		}
	}
}
