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
using System.Net;
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
			var user = await _signInManager.UserManager.FindByEmailAsync(authModel.Email);
			if (user == null)
			{
				return Forbid();
			}
			else
			{
				var emailConfirmed = await _signInManager.UserManager.IsEmailConfirmedAsync(user);
				if (!emailConfirmed)
				{
					return BadRequest(new { Message = "Email not confirmed", Error = true });
				}
			}

			var result = await _signInManager.PasswordSignInAsync(authModel.Email, authModel.Password, false, false);

			if (!result.Succeeded)
			{
				return Forbid();
			}

			user.ConexiuniConturi = await _context.ConexiuniConturi.Include(c => c.ProfilCont).FirstOrDefaultAsync(x => x.UserId == user.Id);

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
				return BadRequest(new { Message = "User already exists", Error = true });
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
				return BadRequest(new { Message = "User could not get created", Error = true });
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

			var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await _signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user)));
			var callbackUrl = $"{_configuration["Front-end-url"]}/auth/confirmation-completion?token={code}&email={user.Email}";

			var setConfirmationTokenResult = await _signInManager.UserManager.SetAuthenticationTokenAsync(user, "Emailconfirmation", "Emailconfirmation", code);
			if (setConfirmationTokenResult.Succeeded)
			{
				if (!_emailSender.SendEmail(new string[] { user.Email }, "Confirmation email code", $"Confirma-ti email-ul accesand <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>link-ul acesta</a>."))
				{
					Console.WriteLine("Confirmation Email failed to be sent");
				}
			}

			return Ok(new { Message = "User created, please confirm your email" });
		}
		[HttpGet("refresh-token")]
		public async Task<IActionResult> RefreshToken(string refreshToken, string refreshTokenId)
		{
			ClaimsPrincipal principal = null;
			string userEmail = null;
			try
			{
				principal = _tokenService.GetPrincipalFromExpiredToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));
				userEmail = principal.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return Unauthorized(new { Message = "Refresh token or jwt token invalid", Error = true });
			}

			var user = await _signInManager.UserManager.FindByNameAsync(userEmail);
			user.ConexiuniConturi = await _context.ConexiuniConturi.Include(c => c.ProfilCont).FirstOrDefaultAsync(x => x.UserId == user.Id);

			var userToken = await _signInManager.UserManager.GetAuthenticationTokenAsync(user, "JWT", refreshTokenId);
			if (user == null || !_tokenService.ValidateRefreshToken(JsonConvert.DeserializeObject<JObject>(userToken), refreshToken))
			{
				return Unauthorized(new { Message = "Refresh token or jwt token invalid", Error = true });
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
				return BadRequest(new { Message = "Please provide reset password token", Error = true });
			}
			var user = await _signInManager.UserManager.FindByEmailAsync(authModel.Email);
			if (user == null)
			{
				return BadRequest(new { Message = "User not found", Error = true });
			}
			var dbToken = await _signInManager.UserManager.GetAuthenticationTokenAsync(user, "Passwordreset", "Passwordreset");
			if (string.IsNullOrWhiteSpace(dbToken) || !dbToken.Equals(resetPasswordToken))
			{
				return BadRequest(new { Message = "Token invalid", Error = true });
			}

			await _signInManager.UserManager.RemovePasswordAsync(user);
			var resetResult = await _signInManager.UserManager.AddPasswordAsync(user, authModel.Password);
			if (resetResult.Succeeded)
			{
				await _signInManager.UserManager.RemoveAuthenticationTokenAsync(user, "Passwordreset", "Passwordreset");
				return Ok(new { Message = "Password has been reseted" });
			}
			return BadRequest(new { Message = "Password reset has failed", Error = true });
		}
		[HttpGet("password-reset-token")]
		public async Task<IActionResult> PasswordResetTokenAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return BadRequest(new { Message = "Email is empty", Error = true });
			}
			var user = await _signInManager.UserManager.FindByEmailAsync(email);
			if (user == null)
			{
				return BadRequest(new { Message = "User not found", Error = true });
			}

			if (!string.IsNullOrWhiteSpace(await _signInManager.UserManager.GetAuthenticationTokenAsync(user, "Passwordreset", "Passwordreset")))
			{
				await _signInManager.UserManager.RemoveAuthenticationTokenAsync(user, "Passwordreset", "Passwordreset");
			}
			var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await _signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user)));
			var callbackUrl = $"{_configuration["Front-end-url"]}/auth/reset-password?token={code}&email={user.Email}";

			var setConfirmationTokenResult = await _signInManager.UserManager.SetAuthenticationTokenAsync(user, "Passwordreset", "Passwordreset", code);
			if (setConfirmationTokenResult.Succeeded)
			{
				if (_emailSender.SendEmail(new string[] { email }, "Password reset code", $"Reseteaza-ti parola accesand <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>link-ul acesta</a>."))
				{
					return Ok(new { Message = "Password reset token generated" });
				}
			}
			return BadRequest(new { Message = "Password reset token failed to get generated", Error = true });
		}
		[HttpGet("confirm-email")]
		public async Task<IActionResult> EmailConfirmationAsync(string emailConfirmationToken, string email)
		{
			if (string.IsNullOrWhiteSpace(emailConfirmationToken))
			{
				return BadRequest(new { Message = "Please provide email confirmation token", Error = true });
			}
			var user = await _signInManager.UserManager.FindByEmailAsync(email);
			if (user != null && user.EmailConfirmed)
			{
				return Ok(new { Message = "Email already confirmed", Error = true });
			}
			if (user == null)
			{
				return BadRequest(new { Message = "User not found", Error = true });
			}
			var dbToken = await _signInManager.UserManager.GetAuthenticationTokenAsync(user, "Emailconfirmation", "Emailconfirmation");
			if (string.IsNullOrWhiteSpace(dbToken) || !dbToken.Equals(emailConfirmationToken))
			{
				return BadRequest(new { Message = "Token invalid", Error = true });
			}

			user.EmailConfirmed = true;
			var confirmResult = await _signInManager.UserManager.UpdateAsync(user);
			if (confirmResult.Succeeded)
			{
				await _signInManager.UserManager.RemoveAuthenticationTokenAsync(user, "Emailconfirmation", "Emailconfirmation");
				return Ok(new { Message = "Email has been confirmed" });
			}
			return BadRequest(new { Message = "Email confirmation failed", Error = true });
		}
		[HttpGet("resend-confirmation-email")]
		public async Task<IActionResult> ResendEmailConfirmationAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return BadRequest(new { Message = "Email is empty", Error = true });
			}
			var user = await _signInManager.UserManager.FindByEmailAsync(email);
			if (user == null || user.EmailConfirmed)
			{
				return BadRequest(new { Message = "User not found, or email already confirmed", Error = true });
			}
			if (!string.IsNullOrWhiteSpace(await _signInManager.UserManager.GetAuthenticationTokenAsync(user, "Emailconfirmation", "Emailconfirmation")))
			{
				await _signInManager.UserManager.RemoveAuthenticationTokenAsync(user, "Emailconfirmation", "Emailconfirmation");
			}
			var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await _signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user)));
			var callbackUrl = $"{_configuration["Front-end-url"]}/auth/confirmation-completion?token={code}&email={user.Email}";
			var setConfirmationTokenResult = await _signInManager.UserManager.SetAuthenticationTokenAsync(user, "Emailconfirmation", "Emailconfirmation", code);
			if (setConfirmationTokenResult.Succeeded)
			{
				if (_emailSender.SendEmail(new string[] { email }, "Email confirmation code", $"Confirma-ti email-ul accesand <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>link-ul acesta</a>."))
				{
					return Ok(new { Message = "Email confirmation token generated" });
				}
			}

			return BadRequest(new { Message = "Email confirmation token failed to get generated", Error = true });
		}
		[Authorize]
		[HttpPost("change-password")]
		public async Task<IActionResult> ChangePasswordAsync([FromBody] AuthModel authModel)
		{
			if (authModel == null)
			{
				return BadRequest(new { Message = "Form is empty", Error = true });
			}
			var userEmail = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
			if (userEmail == null)
			{
				return BadRequest(new { Message = "User not found", Error = true });
			}
			var user = await _signInManager.UserManager.FindByEmailAsync(userEmail);
			if (user == null || user.Email != authModel.Email)
			{
				return BadRequest(new { Message = "User not found", Error = true });
			}

			var changePasswordResult = await _signInManager.UserManager.ChangePasswordAsync(user, authModel.Password, authModel.NewPassword);
			if (changePasswordResult.Succeeded)
			{
				return Ok(new { Message = "Password changed" });
			}
			return BadRequest(new { Message = "Password change failed", Error = true });
		}
		[Authorize]
		[HttpPost("update-profile")]
		public async Task<IActionResult> UpdateProfileAsync([FromBody] AuthModel authModel)
		{
			if (authModel == null)
			{
				return BadRequest(new { Message = "Form is empty", Error = true });
			}
			var userEmail = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
			if (userEmail == null)
			{
				return BadRequest(new { Message = "User not found", Error = true });
			}
			var user = await _signInManager.UserManager.FindByEmailAsync(userEmail);
			if (user == null || user.Email != authModel.Email)
			{
				return BadRequest(new { Message = "User not found", Error = true });
			}
			var conexCont = _context.ConexiuniConturi.Include(c => c.ProfilCont).FirstOrDefault(c => c.UserId == user.Id);
			if (conexCont == null)
			{
				return BadRequest(new { Message = "Missing data", Error = true });
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
			return BadRequest(new { Message = "Profile failed to get updated", Error = true });
		}
		[Authorize]
		[HttpPost("delete-account")]
		public async Task<IActionResult> DeleteAccountAsync([FromBody] AuthModel authModel)
		{
			if (authModel == null)
			{
				return BadRequest(new { Message = "Form is empty", Error = true });
			}
			var userEmail = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
			if (userEmail == null)
			{
				return BadRequest(new { Message = "User not found", Error = true });
			}
			var user = await _signInManager.UserManager.FindByEmailAsync(userEmail);
			if (user == null || user.Email != authModel.Email)
			{
				return BadRequest(new { Message = "User not found", Error = true });
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
			return BadRequest(new { Message = "Account could not get deleted", Error = true });
		}
		[Authorize]
		[HttpGet("logout")]
		public async Task<IActionResult> LogoutAsync(string refreshTokenId)
		{
			ClaimsPrincipal principal = null;
			string userEmail = null;
			try
			{
				principal = _tokenService.GetPrincipalFromExpiredToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));
				userEmail = principal.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return BadRequest(new { Message = "Logout invalid", Error = true });
			}
			var user = await _signInManager.UserManager.FindByNameAsync(userEmail);
			var setAuthTokenResult = await _signInManager.UserManager.RemoveAuthenticationTokenAsync(user, "JWT", refreshTokenId);
			return Ok("User logged out from this device");
		}

	}
}
