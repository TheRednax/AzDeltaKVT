﻿// <auto-generated />
using AzDeltaKVT.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AzDeltaKVT.Core.Migrations
{
    [DbContext(typeof(AzDeltaKVTDbContext))]
    partial class AzDeltaKVTDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AzDektaKVT.Model.Gene", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Chromosome")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Start")
                        .HasColumnType("int");

                    b.Property<int>("Stop")
                        .HasColumnType("int");

                    b.Property<string>("UserInfo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Name");

                    b.ToTable("Genes");
                });

            modelBuilder.Entity("AzDektaKVT.Model.GeneVariant", b =>
                {
                    b.Property<string>("NmId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("VariantId")
                        .HasColumnType("int");

                    b.Property<string>("BiologicalEffect")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Classification")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserInfo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("NmId", "VariantId");

                    b.HasIndex("VariantId");

                    b.ToTable("GeneVariants");
                });

            modelBuilder.Entity("AzDektaKVT.Model.NmTranscript", b =>
                {
                    b.Property<string>("NmNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("GeneId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsClinical")
                        .HasColumnType("bit");

                    b.Property<bool>("IsInHouse")
                        .HasColumnType("bit");

                    b.Property<bool>("IsSelect")
                        .HasColumnType("bit");

                    b.HasKey("NmNumber");

                    b.HasIndex("GeneId");

                    b.ToTable("NmTranscripts");
                });

            modelBuilder.Entity("AzDektaKVT.Model.Variant", b =>
                {
                    b.Property<int>("VariantId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("VariantId"));

                    b.Property<string>("Alternative")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Chromosome")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Position")
                        .HasColumnType("int");

                    b.Property<string>("Reference")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserInfo")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("VariantId");

                    b.ToTable("Variants");
                });

            modelBuilder.Entity("AzDektaKVT.Model.GeneVariant", b =>
                {
                    b.HasOne("AzDektaKVT.Model.NmTranscript", "NmTranscript")
                        .WithMany()
                        .HasForeignKey("NmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AzDektaKVT.Model.Variant", "Variant")
                        .WithMany()
                        .HasForeignKey("VariantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NmTranscript");

                    b.Navigation("Variant");
                });

            modelBuilder.Entity("AzDektaKVT.Model.NmTranscript", b =>
                {
                    b.HasOne("AzDektaKVT.Model.Gene", "Gene")
                        .WithMany()
                        .HasForeignKey("GeneId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Gene");
                });
#pragma warning restore 612, 618
        }
    }
}
