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
        [ForeignKey("ConexiuniConturi")]
        [JsonProperty("conexId")]
        public Guid? ConexId { get; set; }

        [ForeignKey("MyConexiuniConturi")]
        [JsonProperty("myConexId")]
        public Guid? MyConexId { get; set; }

        // Relations
        [JsonIgnore]
        public ConexiuniConturi? ConexiuniConturi { get; set; }

        [JsonProperty("myConexiuniConturi")]
        public ConexiuniConturi? MyConexiuniConturi { get; set; }
    }
}
