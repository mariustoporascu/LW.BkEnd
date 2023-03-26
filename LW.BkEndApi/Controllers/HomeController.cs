using LW.BkEndLogic.Commons;
using LW.BkEndModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LW.BkEndApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class HomeController : ControllerBase
	{
		private readonly ITokenService _tokenService;
		private readonly UserManager<User> _userManager;

		public HomeController(ITokenService tokenService, UserManager<User> userManager)
		{
			_tokenService = tokenService;
			_userManager = userManager;
		}
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var user = await _userManager.FindByEmailAsync("sa@sa.com");
			return Ok(new
			{
				Token = await _tokenService.GenerateJwtToken(user),
				RefreshToken = _tokenService.GenerateRefreshToken()
			});
		}
		[HttpPost]
		[Authorize]
		public IActionResult ValidateToken()
		{
			var principal = _tokenService.GetPrincipalFromExpiredToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));
			return Ok(new { isValid = true });
		}
	}
}
