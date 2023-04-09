using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndModel
{
	public class DataProcDocs
	{
		[Key]
		[JsonProperty("id")]
		public Guid Id { get; set; } = Guid.NewGuid();
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
		public Guid? FirmaDiscountId { get; set; }
		[ForeignKey("ConexiuniConturi")]
		[JsonProperty("conexId")]
		public Guid? ConexId { get; set; }

		// Relations
		[JsonIgnore]
		public FirmaDiscount? FirmaDiscount { get; set; }
		[JsonIgnore]
		public ConexiuniConturi? ConexiuniConturi { get; set; }
		[JsonProperty("fisiereDocumente")]
		public FisiereDocumente? FisiereDocumente { get; set; }
	}
}
