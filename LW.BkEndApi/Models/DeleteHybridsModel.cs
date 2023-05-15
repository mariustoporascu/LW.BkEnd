using Newtonsoft.Json;

namespace LW.BkEndApi.Models
{
	public class DeleteHybridsModel
	{
		[JsonProperty("groupsIds")]
		public List<Guid> GroupsIds { get; set; } = new List<Guid>();
	}
}
