using LW.BkEndModel;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LW.BkEndDb
{
	public partial class LwDatabase : IdentityDbContext<User>
	{
		public LwDatabase(DbContextOptions<LwDatabase> options)
			: base(options)
		{
		}
		public DbSet<ConexiuniConturi> ConexiuniConturi { get; set; }
		public DbSet<ProfilCont> ProfilCont { get; set; }
		public DbSet<Hybrid> Hybrid { get; set; }
		public DbSet<PreferinteHybrid> PreferinteHybrid { get; set; }
		public DbSet<FirmaDiscount> FirmaDiscount { get; set; }
		public DbSet<Documente> Documente { get; set; }
		public DbSet<Tranzactii> Tranzactii { get; set; }
		public DbSet<FisiereDocumente> FisiereDocumente { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			OnModelCreatingPartial(modelBuilder);
		}
		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}
