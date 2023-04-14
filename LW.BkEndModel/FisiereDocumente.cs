using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LW.BkEndModel
{
	public class FisiereDocumente
	{
		[Key]
		[JsonProperty("id")]
		public Guid Id { get; set; } = Guid.NewGuid();
		[JsonIgnore]
		public int CIndex { get; set; }
		[JsonProperty("fileName")]
		public string? FileName { get; set; }
		[JsonProperty("fileExtension")]
		public string? FileExtension { get; set; }
		[JsonProperty("identifier")]
		public string Identifier { get; set; } = Guid.NewGuid().ToString();
		[JsonProperty("created")]
		public DateTime Created { get; set; } = DateTime.UtcNow;

		// Foreign Keys
		[ForeignKey("Documente")]
		[JsonProperty("documenteId")]
		public Guid? DocumenteId { get; set; }

		// Relations
		[JsonIgnore]
		public Documente? Documente { get; set; }
	}
}
