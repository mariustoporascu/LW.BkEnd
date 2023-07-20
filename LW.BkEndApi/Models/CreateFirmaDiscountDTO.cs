using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace LW.BkEndApi.Models
{
    public class CreateFirmaDiscountDTO
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("nameAnaf")]
        public string? NameAnaf { get; set; }

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
        public decimal DiscountPercent { get; set; }
    }
}
