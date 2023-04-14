using LW.BkEndModel;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace LW.BkEndLogic.Commons.Interfaces
{
	public interface ITokenService
	{
		Task<string> GenerateJwtToken(User user);
		string GenerateRefreshToken();
		bool ValidateRefreshToken(JObject userToken, string refreshToken);
		ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
	}
}
