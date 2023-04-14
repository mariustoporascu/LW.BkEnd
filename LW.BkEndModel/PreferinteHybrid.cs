using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LW.BkEndModel
{
	public class PreferinteHybrid
	{
		[Key]
		[JsonProperty("id")]
		public Guid Id { get; set; } = Guid.NewGuid();
		[JsonIgnore]
		public int CIndex { get; set; }

		// Foreign Keys
		[ForeignKey("Hybrid")]
		[JsonProperty("hybridId")]
		public Guid? HybridId { get; set; }
		[ForeignKey("ConexiuniConturi")]
		[JsonProperty("conexId")]
		public Guid? ConexId { get; set; }


		// Relations
		[JsonIgnore]
		public Hybrid? Hybrid { get; set; }
		[JsonProperty("conexiuniConturi")]
		public ConexiuniConturi? ConexiuniConturi { get; set; }
	}
}
