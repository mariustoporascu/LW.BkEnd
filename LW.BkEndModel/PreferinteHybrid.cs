using Microsoft.EntityFrameworkCore;
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
	public class PreferinteHybrid
	{
		[Key]
		[JsonProperty("id")]
		public string Id { get; set; } = Guid.NewGuid().ToString();
		// Foreign Keys
		[ForeignKey("Hybrid")]
		[JsonProperty("hybridId")]
		public string? HybridId { get; set; }
		[ForeignKey("ConexiuniConturi")]
		[JsonProperty("conexId")]
		public string? ConexId { get; set; }


		// Relations
		[JsonIgnore]
		public Hybrid? Hybrid { get; set; }
		[JsonIgnore]
		public ConexiuniConturi? ConexiuniConturi { get; set; }
	}
}
