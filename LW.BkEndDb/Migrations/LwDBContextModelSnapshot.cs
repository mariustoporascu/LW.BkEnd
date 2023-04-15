﻿// <auto-generated />
using System;
using LW.BkEndDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace LW.BkEndDb.Migrations
{
    [DbContext(typeof(LwDBContext))]
    partial class LwDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("LW.BkEndModel.ConexiuniConturi", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CIndex")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CIndex"));

                    b.Property<Guid?>("FirmaDiscountId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("HybridId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("CIndex")
                        .IsUnique();

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CIndex"));

                    b.HasIndex("FirmaDiscountId");

                    b.HasIndex("HybridId");

                    b.HasIndex("UserId")
                        .IsUnique()
                        .HasFilter("[UserId] IS NOT NULL");

                    b.ToTable("ConexiuniConturi");
                });

            modelBuilder.Entity("LW.BkEndModel.Documente", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CIndex")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CIndex"));

                    b.Property<Guid?>("ConexId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("DiscountValue")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("DocNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ExtractedBusinessAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ExtractedBusinessData")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("FirmaDiscountId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsInvoice")
                        .HasColumnType("bit");

                    b.Property<Guid?>("NextConexId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ReceiptId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("StatusName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Total")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("Uploaded")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("CIndex")
                        .IsUnique();

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CIndex"));

                    b.HasIndex("ConexId");

                    b.HasIndex("FirmaDiscountId");

                    b.ToTable("Documente");
                });

            modelBuilder.Entity("LW.BkEndModel.FirmaDiscount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BankAccount")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BankName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CIndex")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CIndex"));

                    b.Property<string>("CuiNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("DiscountPercent")
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("MainContactEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MainContactName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MainContactPhone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NrRegCom")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("TotalGivenDiscount")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("CIndex")
                        .IsUnique();

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CIndex"));

                    b.ToTable("FirmaDiscount");
                });

            modelBuilder.Entity("LW.BkEndModel.FisiereDocumente", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CIndex")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CIndex"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("DocumenteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("FileExtension")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Identifier")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("CIndex")
                        .IsUnique();

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CIndex"));

                    b.HasIndex("DocumenteId")
                        .IsUnique()
                        .HasFilter("[DocumenteId] IS NOT NULL");

                    b.ToTable("FisiereDocumente");
                });

            modelBuilder.Entity("LW.BkEndModel.Hybrid", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CIndex")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CIndex"));

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NoDocsUploaded")
                        .HasColumnType("int");

                    b.Property<int>("NoSubAccounts")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("CIndex")
                        .IsUnique();

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CIndex"));

                    b.ToTable("Hybrid");
                });

            modelBuilder.Entity("LW.BkEndModel.PreferinteHybrid", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CIndex")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CIndex"));

                    b.Property<Guid?>("ConexId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("HybridId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("CIndex")
                        .IsUnique();

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CIndex"));

                    b.HasIndex("ConexId");

                    b.HasIndex("HybridId");

                    b.ToTable("PreferinteHybrid");
                });

            modelBuilder.Entity("LW.BkEndModel.ProfilCont", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CIndex")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CIndex"));

                    b.Property<Guid?>("ConexId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsBusiness")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NoDocsUploaded")
                        .HasColumnType("int");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("CIndex")
                        .IsUnique();

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CIndex"));

                    b.HasIndex("ConexId")
                        .IsUnique()
                        .HasFilter("[ConexId] IS NOT NULL");

                    b.ToTable("ProfilCont");
                });

            modelBuilder.Entity("LW.BkEndModel.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("LW.BkEndModel.Tranzactii", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("CIndex")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CIndex"));

                    b.Property<Guid?>("ConexId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("DocumenteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<string>("TypeName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    SqlServerKeyBuilderExtensions.IsClustered(b.HasKey("Id"), false);

                    b.HasIndex("CIndex")
                        .IsUnique();

                    SqlServerIndexBuilderExtensions.IsClustered(b.HasIndex("CIndex"));

                    b.HasIndex("ConexId");

                    b.HasIndex("DocumenteId");

                    b.ToTable("Tranzactii");
                });

            modelBuilder.Entity("LW.BkEndModel.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("LW.BkEndModel.ConexiuniConturi", b =>
                {
                    b.HasOne("LW.BkEndModel.FirmaDiscount", "FirmaDiscount")
                        .WithMany("ConexiuniConturi")
                        .HasForeignKey("FirmaDiscountId");

                    b.HasOne("LW.BkEndModel.Hybrid", "Hybrid")
                        .WithMany("ConexiuniConturi")
                        .HasForeignKey("HybridId");

                    b.HasOne("LW.BkEndModel.User", "User")
                        .WithOne("ConexiuniConturi")
                        .HasForeignKey("LW.BkEndModel.ConexiuniConturi", "UserId");

                    b.Navigation("FirmaDiscount");

                    b.Navigation("Hybrid");

                    b.Navigation("User");
                });

            modelBuilder.Entity("LW.BkEndModel.Documente", b =>
                {
                    b.HasOne("LW.BkEndModel.ConexiuniConturi", "ConexiuniConturi")
                        .WithMany("Documente")
                        .HasForeignKey("ConexId");

                    b.HasOne("LW.BkEndModel.FirmaDiscount", "FirmaDiscount")
                        .WithMany("Documente")
                        .HasForeignKey("FirmaDiscountId");

                    b.Navigation("ConexiuniConturi");

                    b.Navigation("FirmaDiscount");
                });

            modelBuilder.Entity("LW.BkEndModel.FisiereDocumente", b =>
                {
                    b.HasOne("LW.BkEndModel.Documente", "Documente")
                        .WithOne("FisiereDocumente")
                        .HasForeignKey("LW.BkEndModel.FisiereDocumente", "DocumenteId");

                    b.Navigation("Documente");
                });

            modelBuilder.Entity("LW.BkEndModel.PreferinteHybrid", b =>
                {
                    b.HasOne("LW.BkEndModel.ConexiuniConturi", "ConexiuniConturi")
                        .WithMany("PreferinteHybrid")
                        .HasForeignKey("ConexId");

                    b.HasOne("LW.BkEndModel.Hybrid", "Hybrid")
                        .WithMany("PreferinteHybrid")
                        .HasForeignKey("HybridId");

                    b.Navigation("ConexiuniConturi");

                    b.Navigation("Hybrid");
                });

            modelBuilder.Entity("LW.BkEndModel.ProfilCont", b =>
                {
                    b.HasOne("LW.BkEndModel.ConexiuniConturi", "ConexiuniConturi")
                        .WithOne("ProfilCont")
                        .HasForeignKey("LW.BkEndModel.ProfilCont", "ConexId");

                    b.Navigation("ConexiuniConturi");
                });

            modelBuilder.Entity("LW.BkEndModel.Tranzactii", b =>
                {
                    b.HasOne("LW.BkEndModel.ConexiuniConturi", "ConexiuniConturi")
                        .WithMany("Tranzactii")
                        .HasForeignKey("ConexId");

                    b.HasOne("LW.BkEndModel.Documente", "Documente")
                        .WithMany("Tranzactii")
                        .HasForeignKey("DocumenteId");

                    b.Navigation("ConexiuniConturi");

                    b.Navigation("Documente");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.HasOne("LW.BkEndModel.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.HasOne("LW.BkEndModel.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.HasOne("LW.BkEndModel.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.HasOne("LW.BkEndModel.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LW.BkEndModel.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.HasOne("LW.BkEndModel.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("LW.BkEndModel.ConexiuniConturi", b =>
                {
                    b.Navigation("Documente");

                    b.Navigation("PreferinteHybrid");

                    b.Navigation("ProfilCont");

                    b.Navigation("Tranzactii");
                });

            modelBuilder.Entity("LW.BkEndModel.Documente", b =>
                {
                    b.Navigation("FisiereDocumente");

                    b.Navigation("Tranzactii");
                });

            modelBuilder.Entity("LW.BkEndModel.FirmaDiscount", b =>
                {
                    b.Navigation("ConexiuniConturi");

                    b.Navigation("Documente");
                });

            modelBuilder.Entity("LW.BkEndModel.Hybrid", b =>
                {
                    b.Navigation("ConexiuniConturi");

                    b.Navigation("PreferinteHybrid");
                });

            modelBuilder.Entity("LW.BkEndModel.User", b =>
                {
                    b.Navigation("ConexiuniConturi");
                });
#pragma warning restore 612, 618
        }
    }
}
