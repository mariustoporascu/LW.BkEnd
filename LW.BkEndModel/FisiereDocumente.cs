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
	public class FisiereDocumente
	{
		[Key]
		[JsonProperty("id")]
		public Guid Id { get; set; } = Guid.NewGuid();
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
		[ForeignKey("DataProcDocs")]
		[JsonProperty("dataProcDocsId")]
		public Guid? DataProcDocsId { get; set; }

		// Relations
		[JsonIgnore]
		public Documente? Documente { get; set; }
		[JsonIgnore]
		public DataProcDocs? DataProcDocs { get; set; }
	}
}
