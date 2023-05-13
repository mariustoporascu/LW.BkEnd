using LW.BkEndModel.Enums;
using Newtonsoft.Json;

namespace LW.BkEndApi.Models
{
	public class TranzactionModel
	{
		//List<Guid> documenteIds, Guid? nextConexId, TranzactionTypeEnum tranzactionType
		[JsonProperty("documenteIds")]
		public List<Guid> DocumenteIds { get; set; } = new List<Guid>();
		[JsonProperty("nextConexId")]
		public Guid? NextConexId { get; set; }
		[JsonProperty("tranzactionType")]
		public TranzactionTypeEnum TranzactionType { get; set; }
	}
}
