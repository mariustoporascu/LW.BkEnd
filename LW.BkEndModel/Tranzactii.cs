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
		public Guid Id { get; set; } = Guid.NewGuid();
		[JsonProperty("isWithdraw")]
		public bool isWithdraw { get; set; } = false;
		[Column(TypeName = "decimal(18,2)")]
		[JsonProperty("amount")]
		public decimal Amount { get; set; }
		[JsonProperty("created")]
		public DateTime Created { get; set; } = DateTime.UtcNow;

		// Foreign Keys
		[ForeignKey("Documente")]
		[JsonProperty("documenteId")]
		public Guid? DocumenteId { get; set; }
		[ForeignKey("ConexiuniConturi")]
		[JsonProperty("conexId")]
		public Guid? ConexId { get; set; }

		// Relations
		[JsonIgnore]
		public Documente? Documente { get; set; }
		[JsonIgnore]
		public ConexiuniConturi? ConexiuniConturi { get; set; }
	}
}
