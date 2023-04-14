using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace LW.BkEndModel
{
	public class Hybrid
	{
		[Key]
		[JsonProperty("id")]
		public Guid Id { get; set; } = Guid.NewGuid();
		[JsonIgnore]
		public int CIndex { get; set; }
		[JsonProperty("name")]
		public string? Name { get; set; }
		[JsonProperty("noSubAccounts")]
		public int NoSubAccounts { get; set; }
		[JsonProperty("noDocsUploaded")]
		public int NoDocsUploaded { get; set; }

		// Relations
		[JsonProperty("conexiuniConturi")]
		public ICollection<ConexiuniConturi>? ConexiuniConturi { get; set; }
		[JsonProperty("preferinteHybrid")]
		public ICollection<PreferinteHybrid>? PreferinteHybrid { get; set; }
	}
}
