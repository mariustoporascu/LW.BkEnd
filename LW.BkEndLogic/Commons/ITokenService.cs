using LW.BkEndModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndLogic.Commons
{
	public interface ITokenService
	{
		Task<string> GenerateJwtToken(User user);
		string GenerateRefreshToken();
		bool ValidateRefreshToken(JObject userToken, string refreshToken);
		ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
	}
	public class TokenService : ITokenService
	{
		private readonly IConfiguration _configuration;
		private readonly UserManager<User> _userManager;
		private readonly RsaSecurityKey _key;

		public TokenService(IConfiguration configuration, UserManager<User> userManager)
		{
			_configuration = configuration;
			_userManager = userManager;
			var rsaKey = RSA.Create();
			rsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);
			_key = new RsaSecurityKey(rsaKey);
		}
		private async Task<string> GetUserType(User user)
		{
			string userType = string.Empty;
			if (await _userManager.IsInRoleAsync(user, "master-administrator"))
			{
				userType = "master-admin";
			}
			else if (user.ConexiuniConturi?.FirmaDiscountId != null)
			{
				userType = "firma-admin";
			}
			else if (user.ConexiuniConturi?.HybridId != null)
			{
				userType = "hybrid-admin";
			}
			else
			{
				userType = "user";
			}
			return userType;
		}
		public async Task<string> GenerateJwtToken(User user)
		{
			var credentials = new SigningCredentials(_key, SecurityAlgorithms.RsaSha256);
			var userType = await GetUserType(user);
			var claims = new List<Claim>
				{
					new Claim("sub", user.Id.ToString()),
					new Claim("name", user.ConexiuniConturi?.ProfilCont?.Name ?? ""),
					new Claim("firstName", user.ConexiuniConturi?.ProfilCont?.FirstName ?? ""),
					new Claim("email", user.Email),
					new Claim("phone", user.PhoneNumber ?? ""),
					new Claim("type", userType),
				};
			var roles = await _userManager.GetRolesAsync(user);
			foreach (var role in roles)
			{
				claims.Add(new Claim("role", role));
			}
			var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
				_configuration["Jwt:Issuer"],
				claims,
				expires: DateTime.Now.AddMinutes(int.Parse(_configuration["Jwt:TokenLifeTime"])),
				signingCredentials: credentials);
			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public string GenerateRefreshToken()
		{
			var randomNumber = new byte[32];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomNumber);
			return WebEncoders.Base64UrlEncode(randomNumber);
		}

		public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
		{
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateIssuerSigningKey = true,
				ValidateLifetime = false,
				ValidIssuer = _configuration["Jwt:Issuer"],
				ValidAudience = _configuration["Jwt:Issuer"],
				IssuerSigningKey = _key,
			};
			var tokenHandler = new JwtSecurityTokenHandler()
			{
				MapInboundClaims = false
			};
			SecurityToken securityToken;
			var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
			var jwtSecurityToken = securityToken as JwtSecurityToken;
			if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new SecurityTokenException("Invalid token");
			}
			return principal;
		}

		public bool ValidateRefreshToken(JObject userToken, string refreshToken)
		{
			return userToken["refreshToken"] != null &&
				refreshToken.Equals((string)userToken["refreshToken"]) &&
				DateTime.Parse((string)userToken["refreshTokenExpiry"]) > DateTime.UtcNow;
		}
	}
}
