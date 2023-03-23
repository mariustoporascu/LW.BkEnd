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
	public class Documente
	{
		[Key]
		[JsonProperty("id")]
		public string Id { get; set; } = Guid.NewGuid().ToString();
		[JsonProperty("docNumber")]
		public string? DocNumber { get; set; }
		[JsonProperty("total")]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Total { get; set; }
		[JsonProperty("isInvoice")]
		public bool IsInvoice { get; set; } = false;
		[JsonProperty("isApproved")]
		public bool IsApproved { get; set; } = false;
		[JsonProperty("receiptId")]
		public string? ReceiptId { get; set; }
		[JsonProperty("discountValue")]
		[Column(TypeName = "decimal(18,2)")]
		public decimal DiscountValue { get; set; }
		[JsonProperty("extractedBusinessData")]
		public string? ExtractedBusinessData { get; set; }
		[JsonProperty("extractedBusinessAddress")]
		public string? ExtractedBusinessAddress { get; set; }
		[JsonProperty("uploaded")]
		public DateTime Uploaded { get; set; } = DateTime.UtcNow;

		// Foreign Keys
		[ForeignKey("FirmaDiscount")]
		[JsonProperty("firmaDiscountId")]
		public string? FirmaDiscountId { get; set; }
		[ForeignKey("ConexiuniConturi")]
		[JsonProperty("conexId")]
		public string? ConexId { get; set; }
		[JsonProperty("nextConexId")]
		public string? NextConexId { get; set; }

		// Relations
		[JsonIgnore]
		public FirmaDiscount? FirmaDiscount { get; set; }
		[JsonIgnore]
		public ConexiuniConturi? ConexiuniConturi { get; set; }
		[JsonIgnore]
		public FisiereDocumente? FisiereDocumente { get; set; }
	}
}
