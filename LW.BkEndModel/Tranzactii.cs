using LW.BkEndModel.Enums;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LW.BkEndModel
{
	public class Tranzactii
	{
		[Key]
		[JsonProperty("id")]
		public Guid Id { get; set; } = Guid.NewGuid();
		[JsonIgnore]
		public int CIndex { get; set; }
		[JsonProperty("type")]
		public int Type { get; set; } = 0;
		[JsonProperty("typeName")]
		public string? TypeName { get; set; } = TransferTypeEnum.NoStatus.ToString();
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
