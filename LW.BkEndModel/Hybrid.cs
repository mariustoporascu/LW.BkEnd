using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
		// Foreign Keys
		[ForeignKey("FirmaDiscount")]
		[JsonProperty("firmaDiscountId")]
		public Guid? FirmaDiscountId { get; set; }

		// Relations
		[JsonIgnore]
		public FirmaDiscount? FirmaDiscount { get; set; }
		[JsonProperty("conexiuniConturi")]
		public ICollection<ConexiuniConturi>? ConexiuniConturi { get; set; }
		[JsonProperty("preferinteHybrid")]
		public ICollection<PreferinteHybrid>? PreferinteHybrid { get; set; }
	}
}
