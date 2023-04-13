using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndModel
{
	public class ProfilCont
	{
		[Key]
		[JsonProperty("id")]
		public Guid Id { get; set; } = Guid.NewGuid();
		[JsonIgnore]
		public int CIndex { get; set; }
		[JsonProperty("name")]
		public string? Name { get; set; }
		[JsonProperty("firstName")]
		public string? FirstName { get; set; }
		[JsonProperty("email")]
		public string? Email { get; set; }
		[JsonProperty("phoneNumber")]
		public string? PhoneNumber { get; set; }
		[JsonProperty("noDocsUploaded")]
		public int NoDocsUploaded { get; set; }
		[JsonProperty("isBusiness")]
		public bool IsBusiness { get; set; } = false;

		// Foreign keys
		[ForeignKey("ConexiuniConturi")]
		[JsonProperty("conexId")]
		public Guid? ConexId { get; set; }

		// Relations
		[JsonIgnore]
		public ConexiuniConturi? ConexiuniConturi { get; set; }
	}
}
