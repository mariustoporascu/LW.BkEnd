using LW.BkEndModel;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LW.BkEndDb
{
	public partial class LwDBContext : IdentityDbContext<User, Role, Guid>
	{
		public LwDBContext(DbContextOptions<LwDBContext> options)
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
			modelBuilder.Entity<ConexiuniConturi>(ConfigureConexiuniConturi);
			modelBuilder.Entity<ProfilCont>(ConfigureProfilCont);
			modelBuilder.Entity<Hybrid>(ConfigureHybrid);
			modelBuilder.Entity<FirmaDiscount>(ConfigureFirmaDiscount);
			modelBuilder.Entity<Documente>(ConfigureDocumente);
			modelBuilder.Entity<Tranzactii>(ConfigureTranzactii);
			modelBuilder.Entity<FisiereDocumente>(ConfigureFisiereDocumente);
			modelBuilder.Entity<PreferinteHybrid>(ConfigurePreferinteHybrid);

			OnModelCreatingPartial(modelBuilder);
		}
		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
		private void ConfigureConexiuniConturi(EntityTypeBuilder<ConexiuniConturi> builder)
		{
			// Configure primary key as non-clustered
			builder.HasKey(cc => cc.Id).IsClustered(false);
			builder.HasMany(cc => cc.Documente).WithOne(d => d.ConexiuniConturi).HasForeignKey(d => d.ConexId);
			builder.HasOne(d => d.User)
				.WithOne(cc => cc.ConexiuniConturi)
				.HasForeignKey<ConexiuniConturi>(d => d.UserId)
				.OnDelete(DeleteBehavior.SetNull);
			builder.HasOne(d => d.Hybrid)
				.WithMany(cc => cc.ConexiuniConturi)
				.HasForeignKey(cc => cc.HybridId)
				.OnDelete(DeleteBehavior.SetNull);

			// Configure clustered index for the ClusteredIndex property
			builder.Property(cc => cc.CIndex).UseIdentityColumn();
			builder.Property(cc => cc.CIndex).ValueGeneratedOnAddOrUpdate();
			builder.HasIndex(cc => cc.CIndex).IsUnique().IsClustered();
		}
		private void ConfigureProfilCont(EntityTypeBuilder<ProfilCont> builder)
		{
			// Configure primary key as non-clustered
			builder.HasKey(cc => cc.Id).IsClustered(false);

			// Configure clustered index for the ClusteredIndex property
			builder.Property(cc => cc.CIndex).UseIdentityColumn();
			builder.Property(cc => cc.CIndex).ValueGeneratedOnAddOrUpdate();
			builder.HasIndex(cc => cc.CIndex).IsUnique().IsClustered();
		}
		private void ConfigureHybrid(EntityTypeBuilder<Hybrid> builder)
		{
			// Configure primary key as non-clustered
			builder.HasKey(cc => cc.Id).IsClustered(false);

			// Configure clustered index for the ClusteredIndex property
			builder.Property(cc => cc.CIndex).UseIdentityColumn();
			builder.Property(cc => cc.CIndex).ValueGeneratedOnAddOrUpdate();
			builder.HasIndex(cc => cc.CIndex).IsUnique().IsClustered();
		}
		private void ConfigureFirmaDiscount(EntityTypeBuilder<FirmaDiscount> builder)
		{
			// Configure primary key as non-clustered
			builder.HasKey(cc => cc.Id).IsClustered(false);

			// Configure clustered index for the ClusteredIndex property
			builder.Property(cc => cc.CIndex).UseIdentityColumn();
			builder.Property(cc => cc.CIndex).ValueGeneratedOnAddOrUpdate();
			builder.HasIndex(cc => cc.CIndex).IsUnique().IsClustered();
		}
		private void ConfigureDocumente(EntityTypeBuilder<Documente> builder)
		{
			// Configure primary key as non-clustered
			builder.HasKey(cc => cc.Id).IsClustered(false);

			// Configure clustered index for the ClusteredIndex property
			builder.Property(cc => cc.CIndex).UseIdentityColumn();
			builder.Property(cc => cc.CIndex).ValueGeneratedOnAddOrUpdate();
			builder.HasIndex(cc => cc.CIndex).IsUnique().IsClustered();
		}
		private void ConfigureTranzactii(EntityTypeBuilder<Tranzactii> builder)
		{
			// Configure primary key as non-clustered
			builder.HasKey(cc => cc.Id).IsClustered(false);

			// Configure clustered index for the ClusteredIndex property
			builder.Property(cc => cc.CIndex).UseIdentityColumn();
			builder.Property(cc => cc.CIndex).ValueGeneratedOnAddOrUpdate();
			builder.HasIndex(cc => cc.CIndex).IsUnique().IsClustered();
		}
		private void ConfigurePreferinteHybrid(EntityTypeBuilder<PreferinteHybrid> builder)
		{
			// Configure primary key as non-clustered
			builder.HasKey(cc => cc.Id).IsClustered(false);

			// Configure clustered index for the ClusteredIndex property
			builder.Property(cc => cc.CIndex).UseIdentityColumn();
			builder.Property(cc => cc.CIndex).ValueGeneratedOnAddOrUpdate();
			builder.HasIndex(cc => cc.CIndex).IsUnique().IsClustered();
		}
		private void ConfigureFisiereDocumente(EntityTypeBuilder<FisiereDocumente> builder)
		{
			// Configure primary key as non-clustered
			builder.HasKey(cc => cc.Id).IsClustered(false);

			// Configure clustered index for the ClusteredIndex property
			builder.Property(cc => cc.CIndex).UseIdentityColumn();
			builder.Property(cc => cc.CIndex).ValueGeneratedOnAddOrUpdate();
			builder.HasIndex(cc => cc.CIndex).IsUnique().IsClustered();
		}
	}
}
