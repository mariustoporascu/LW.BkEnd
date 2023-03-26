using LW.BkEndApi.Models;
using LW.BkEndDb;
using LW.BkEndLogic.Commons;
using LW.BkEndModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace LW.BkEndApi.Controllers
{
	[Route("[controller]")]
	[ApiController]
	[AllowAnonymous]
	public class AuthController : ControllerBase
	{
		private readonly ITokenService _tokenService;
		private readonly SignInManager<User> _signInManager;
		private readonly LwDBContext _context;
		private readonly IEmailSender _emailSender;
		private readonly IConfiguration _configuration;

		public AuthController(ITokenService tokenService,
			SignInManager<User> signInManager,
			LwDBContext context,
			IEmailSender emailSender,
			IConfiguration configuration)
		{
			_tokenService = tokenService;
			_signInManager = signInManager;
			_context = context;
			_emailSender = emailSender;
			_configuration = configuration;
		}
		[HttpPost("login")]
		public async Task<IActionResult> LoginAsync([FromBody] AuthModel authModel)
		{
			var result = await _signInManager.PasswordSignInAsync(authModel.Email, authModel.Password, false, false);

			if (!result.Succeeded)
			{
				return Unauthorized();
			}

			var user = await _signInManager.UserManager.FindByEmailAsync(authModel.Email);

			var token = await _tokenService.GenerateJwtToken(user);
			var refreshToken = _tokenService.GenerateRefreshToken();
			var refreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:ResetTokenLifeTime"]));

			var identityToken = new
			{
				LoginProvider = "JWT",
				Name = Guid.NewGuid().ToString(),
				Value = JsonConvert.SerializeObject(new
				{
					refreshToken,
					refreshTokenExpiry
				}),
			};
			// save identityToken to database
			var setAuthTokenResult = await _signInManager.UserManager.SetAuthenticationTokenAsync(user, identityToken.LoginProvider, identityToken.Name, identityToken.Value);
			if (!setAuthTokenResult.Succeeded)
			{
				Console.WriteLine("Auth token could not get saved");
			}
			else
			{
				await _signInManager.UserManager.UpdateAsync(user);
			}

			return Ok(new
			{
				Token = token,
				RefreshTokenId = identityToken.Name,
				RefreshToken = refreshToken
			});
		}
		[HttpPost("register")]
		public async Task<IActionResult> RegisterAsync([FromBody] AuthModel authModel)
		{
			//verify if user already exists
			if (await _signInManager.UserManager.FindByEmailAsync(authModel.Email) != null)
			{
				return Ok(new { Message = "User already exists", Error = true });
			}
			var user = new User
			{
				Email = authModel.Email,
				UserName = authModel.Email,
				PhoneNumber = authModel.PhoneNumber
			};
			var result = await _signInManager.UserManager.CreateAsync(user, authModel.Password);
			if (!result.Succeeded)
			{
				return Ok(new { Message = "User could not get created", Error = true });
			}

			var conexCont = new ConexiuniConturi
			{
				UserId = user.Id,
				ProfilCont = new ProfilCont
				{
					Email = user.Email,
					Name = authModel.Name,
					FirstName = authModel.FirstName,
					PhoneNumber = authModel.PhoneNumber,
					IsBusiness = authModel.isBusiness
				}
			};
			_context.Add(conexCont);
			await _context.SaveChangesAsync();

			var code = await _signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);
			if (!_emailSender.SendEmail(new string[] { user.Email }, "Confirmation email code", HtmlEncoder.Default.Encode(code)))
			{
				Console.WriteLine("Confirmation Email failed to be sent");
			}

			return Ok(new { Message = "User created, please confirm your email" });
		}
		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshToken(string refreshToken, string refreshTokenId)
		{
			var principal = _tokenService.GetPrincipalFromExpiredToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));
			var userEmail = principal.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
			if (userEmail == null)
			{
				return Unauthorized();
			}
			var user = await _signInManager.UserManager.FindByNameAsync(userEmail);
			var userToken = await _signInManager.UserManager.GetAuthenticationTokenAsync(user, "JWT", refreshTokenId);
			if (user == null || !_tokenService.ValidateRefreshToken(JsonConvert.DeserializeObject<JObject>(userToken), refreshToken))
			{
				return Unauthorized();
			}

			var token = await _tokenService.GenerateJwtToken(user);

			return Ok(new
			{
				Token = token,
			});
		}
		[HttpPost("password-reset")]
		public async Task<IActionResult> PasswordResetAsync(string resetPasswordToken, [FromBody] AuthModel authModel)
		{
			if (string.IsNullOrWhiteSpace(resetPasswordToken))
			{
				return Unauthorized();
			}
			var resetCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordToken));
			var user = await _signInManager.UserManager.FindByEmailAsync(authModel.Email);
			if (user == null)
			{
				return Unauthorized();
			}
			var resetResult = await _signInManager.UserManager.ResetPasswordAsync(user, resetCode, authModel.Password);
			if (resetResult.Succeeded)
			{
				return Ok(new { Message = "Password has been reseted" });
			}
			return Ok(new { Message = "Password reset has failed", Error = true });
		}
		[HttpPost("password-reset-token")]
		public async Task<IActionResult> PasswordResetTokenAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return Unauthorized();
			}
			var user = await _signInManager.UserManager.FindByEmailAsync(email);
			if (user == null)
			{
				return Unauthorized();
			}
			var code = await _signInManager.UserManager.GeneratePasswordResetTokenAsync(user);
			if (_emailSender.SendEmail(new string[] { email }, "Password reset code", HtmlEncoder.Default.Encode(code)))
			{
				return Ok(new { Message = "Password reset token generated" });
			}
			return Ok(new { Message = "Password reset token failed to get generated", Error = true });
		}
		[Authorize]
		[HttpPost("change-password")]
		public async Task<IActionResult> ChangePasswordAsync([FromBody] AuthModel authModel)
		{
			if (authModel == null)
			{
				return Unauthorized();
			}
			var user = await _signInManager.UserManager.FindByEmailAsync(authModel.Email);
			var attemptSignInResult = await _signInManager.CheckPasswordSignInAsync(user, authModel.Password, false);
			if (!attemptSignInResult.Succeeded)
			{
				return Unauthorized();
			}
			var changePasswordResult = await _signInManager.UserManager.ChangePasswordAsync(user, authModel.Password, authModel.NewPassword);
			if (changePasswordResult.Succeeded)
			{
				return Ok(new { Message = "Password changed" });
			}
			return Ok(new { Message = "Password change failed", Error = true });
		}
		[Authorize]
		[HttpPost("update-profile")]
		public async Task<IActionResult> UpdateProfileAsync([FromBody] AuthModel authModel)
		{
			if (authModel == null)
			{
				return Unauthorized();
			}
			var user = await _signInManager.UserManager.FindByEmailAsync(authModel.Email);
			if (user == null)
			{
				return Unauthorized();
			}
			var conexCont = _context.ConexiuniConturi.Include(c => c.ProfilCont).FirstOrDefault(c => c.UserId == user.Id);
			if (conexCont == null)
			{
				return NotFound();
			}
			user.PhoneNumber = authModel.PhoneNumber ?? user.PhoneNumber;
			conexCont.ProfilCont.Name = authModel.Name ?? conexCont.ProfilCont.Name;
			conexCont.ProfilCont.FirstName = authModel.FirstName ?? conexCont.ProfilCont.FirstName;
			conexCont.ProfilCont.PhoneNumber = authModel.PhoneNumber ?? conexCont.ProfilCont.PhoneNumber;
			await _signInManager.UserManager.UpdateAsync(user);
			_context.Update(conexCont);
			var updateResult = await _context.SaveChangesAsync();
			if (updateResult > 0)
			{
				return Ok(new { Message = "Profile updated" });
			}
			return Ok(new { Message = "Profile failed to get updated", Error = true });
		}
		[Authorize]
		[HttpPost("delete-account")]
		public async Task<IActionResult> DeleteAccountAsync([FromBody] AuthModel authModel)
		{
			if (authModel == null)
			{
				return Unauthorized();
			}
			var user = await _signInManager.UserManager.FindByEmailAsync(authModel.Email);
			if (user == null)
			{
				return Unauthorized();
			}
			var conexCont = _context.ConexiuniConturi.FirstOrDefault(c => c.UserId == user.Id);
			if (conexCont != null)
			{
				conexCont.UserId = null;
				_context.Update(conexCont);
				await _context.SaveChangesAsync();
			}
			var deleteResult = await _signInManager.UserManager.DeleteAsync(user);
			if (deleteResult.Succeeded)
			{
				return Ok(new { Message = "Account deleted" });
			}
			return Ok(new { Message = "Account could not get deleted", Error = true });
		}
	}
}
