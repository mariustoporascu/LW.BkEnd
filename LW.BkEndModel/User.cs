using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndModel
{
	public class User : IdentityUser
	{
		//Relations
		[JsonIgnore]
		public ConexiuniConturi? ConexiuniConturi { get; set; }
	}
}
