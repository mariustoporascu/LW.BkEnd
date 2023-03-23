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
	public class Tranzactii
	{
		[Key]
		[JsonProperty("id")]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		public bool isWithdraw { get; set; } = false;
		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; }
		public DateTime Created { get; set; } = DateTime.UtcNow;

		// Foreign Keys
		[ForeignKey("Documente")]
		[JsonProperty("documenteId")]
		public string? DocumenteId { get; set; }
		[ForeignKey("ConexiuniConturi")]
		[JsonProperty("conexId")]
		public string? ConexId { get; set; }

		// Relations
		[JsonIgnore]
		public Documente? Documente { get; set; }
		[JsonIgnore]
		public ConexiuniConturi? ConexiuniConturi { get; set; }
	}
}
