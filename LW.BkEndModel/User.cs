using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace LW.BkEndModel
{
	public class User : IdentityUser<Guid>
	{
		//Relations
		[JsonIgnore]
		public ConexiuniConturi? ConexiuniConturi { get; set; }
	}
}
