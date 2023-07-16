using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LW.BkEndModel
{
    public class FirmaDiscount
    {
        [Key]
        [JsonProperty("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [JsonIgnore]
        public int CIndex { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("cuiNumber")]
        public string? CuiNumber { get; set; }

        [JsonProperty("nrRegCom")]
        public string? NrRegCom { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("bankName")]
        public string? BankName { get; set; }

        [JsonProperty("bankAccount")]
        public string? BankAccount { get; set; }

        [JsonProperty("mainContactName")]
        public string? MainContactName { get; set; }

        [JsonProperty("mainContactEmail")]
        public string? MainContactEmail { get; set; }

        [JsonProperty("mainContactPhone")]
        public string? MainContactPhone { get; set; }

        [JsonProperty("discountPercent")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountPercent { get; set; }

        [JsonProperty("totalGivenDiscount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalGivenDiscount { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; } = false;

        [JsonProperty("isActiveSecondary")]
        public bool IsActiveSecondary { get; set; } = false;

        // Relations
        [JsonProperty("conexiuniConturi")]
        public ICollection<ConexiuniConturi>? ConexiuniConturi { get; set; }

        [JsonProperty("documente")]
        public ICollection<Documente>? Documente { get; set; }

        [JsonProperty("hybrid")]
        public ICollection<Hybrid>? Hybrid { get; set; }
    }
}
