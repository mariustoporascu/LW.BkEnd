using LW.BkEndModel.Enums;
using Newtonsoft.Json;

namespace LW.BkEndApi.Models
{
	public class DocStatusUpdateModel
	{
		[JsonProperty("documenteIds")]
		public List<Guid> DocumenteIds { get; set; } = new List<Guid>();
		[JsonProperty("status")]
		public StatusEnum Status { get; set; }
	}
}
