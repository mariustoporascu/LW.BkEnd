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
	public class ConexiuniConturi
	{
		[Key]
		[JsonProperty("id")]
		public Guid Id { get; set; } = Guid.NewGuid();

		// Foreign Keys
		[ForeignKey("User")]
		[JsonProperty("userId")]
		public Guid? UserId { get; set; }
		[ForeignKey("Hybrid")]
		[JsonProperty("hybridId")]
		public Guid? HybridId { get; set; }
		[ForeignKey("FirmaDiscount")]
		[JsonProperty("firmaDiscountId")]
		public Guid? FirmaDiscountId { get; set; }

		// Relations
		[JsonIgnore]
		public User? User { get; set; }
		[JsonIgnore]
		public Hybrid? Hybrid { get; set; }
		[JsonIgnore]
		public FirmaDiscount? FirmaDiscount { get; set; }
		[JsonIgnore]
		public ProfilCont? ProfilCont { get; set; }
		[JsonIgnore]
		public ICollection<Tranzactii>? Tranzactii { get; set; }
		[JsonIgnore]
		public ICollection<Documente>? Documente { get; set; }
		[JsonIgnore]
		public ICollection<PreferinteHybrid>? PreferinteHybrid { get; set; }
	}
}
