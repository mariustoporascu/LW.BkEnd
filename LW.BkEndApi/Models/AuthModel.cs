using Newtonsoft.Json;

namespace LW.BkEndApi.Models
{
	public class AuthModel
	{
		public string? Name { get; set; }
		public string? FirstName { get; set; }
		public string? Email { get; set; }
		public string? Password { get; set; }
		public string? NewPassword { get; set; }
		public bool isBusiness { get; set; } = false;
		public string? PhoneNumber { get; set; }
	}
}
