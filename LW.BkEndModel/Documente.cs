﻿using LW.BkEndModel.Enums;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LW.BkEndModel
{
	public class Documente
	{
		[Key]
		[JsonProperty("id")]
		public Guid Id { get; set; } = Guid.NewGuid();
		[JsonIgnore]
		public int CIndex { get; set; }
		[JsonProperty("docNumber")]
		public string? DocNumber { get; set; }
		[JsonProperty("total")]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Total { get; set; }
		[JsonProperty("isInvoice")]
		public bool IsInvoice { get; set; } = false;
		[JsonProperty("status")]
		public int Status { get; set; } = 0;
		[JsonProperty("statusName")]
		public string? StatusName { get; set; } = StatusEnum.NoStatus.ToString();
		[JsonProperty("receiptId")]
		public string? ReceiptId { get; set; }
		[JsonProperty("discountValue")]
		[Column(TypeName = "decimal(18,2)")]
		public decimal DiscountValue { get; set; }
		[JsonProperty("extractedBusinessData")]
		public string? ExtractedBusinessData { get; set; }
		[JsonProperty("extractedBusinessAddress")]
		public string? ExtractedBusinessAddress { get; set; }
		[JsonProperty("errors")]
		[NotMapped]
		public string? Errors { get; set; }
		[JsonProperty("hasErrors")]
		[NotMapped]
		public bool HasErrors { get; set; } = false;
		[JsonProperty("uploaded")]
		public DateTime Uploaded { get; set; } = DateTime.UtcNow;

		// Foreign Keys
		[ForeignKey("FirmaDiscount")]
		[JsonProperty("firmaDiscountId")]
		public Guid? FirmaDiscountId { get; set; }
		[ForeignKey("ConexiuniConturi")]
		[JsonProperty("conexId")]
		public Guid? ConexId { get; set; }
		[ForeignKey("NextConexiuniConturi")]
		[JsonProperty("nextConexId")]
		public Guid? NextConexId { get; set; }

		// Relations
		[JsonIgnore]
		public FirmaDiscount? FirmaDiscount { get; set; }
		[JsonIgnore]
		public ICollection<Tranzactii>? Tranzactii { get; set; }
		[JsonProperty("conexiuniConturi")]
		public ConexiuniConturi? ConexiuniConturi { get; set; }
		[JsonProperty("nextConexiuniConturi")]
		public ConexiuniConturi? NextConexiuniConturi { get; set; }
		[JsonProperty("fisiereDocumente")]
		public FisiereDocumente? FisiereDocumente { get; set; }
	}
}
