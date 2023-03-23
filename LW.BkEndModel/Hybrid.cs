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
	public class Hybrid
	{
		[Key]
		[JsonProperty("id")]
		public string Id { get; set; } = Guid.NewGuid().ToString();
		[JsonProperty("name")]
		public string? Name { get; set; }
		[JsonProperty("noSubAccounts")]
		public int NoSubAccounts { get; set; }
		[JsonProperty("noDocsUploaded")]
		public int NoDocsUploaded { get; set; }

		// Relations
		[JsonIgnore]
		public ICollection<ConexiuniConturi>? ConexiuniConturi { get; set; }
		[JsonIgnore]
		public ICollection<PreferinteHybrid>? PreferinteHybrid { get; set; }
	}
}
