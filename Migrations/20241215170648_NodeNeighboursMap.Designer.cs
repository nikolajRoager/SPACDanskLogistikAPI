﻿// <auto-generated />
using System;
using DanskLogistikAPI.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DanskLogistikAPI.Migrations
{
    [DbContext(typeof(LogisticContext))]
    [Migration("20241215170648_NodeNeighboursMap")]
    partial class NodeNeighboursMap
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("DanskLogistikAPI.Models.Connection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AId")
                        .HasColumnType("int");

                    b.Property<int>("BId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<TimeSpan?>("Time")
                        .HasColumnType("time(6)");

                    b.Property<int>("mode")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.HasKey("Id");

                    b.HasIndex("AId");

                    b.HasIndex("BId");

                    b.ToTable("Connections");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.Consumer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<int>("NodeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Consumers");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.Country", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Access")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("DeJureOutlineId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("DeJureOutlineId");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.Municipality", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ControllerId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<int>("OutlineId")
                        .HasColumnType("int");

                    b.Property<int>("OwnerId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ControllerId");

                    b.HasIndex("OutlineId");

                    b.HasIndex("OwnerId");

                    b.ToTable("municipalities");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.Node", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("LocationId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<bool>("isAirport")
                        .HasColumnType("tinyint(1)");

                    b.Property<float>("x")
                        .HasColumnType("float");

                    b.Property<float>("y")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("LocationId");

                    b.ToTable("Nodes");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.NodeMapping", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ConnectionId")
                        .HasColumnType("int");

                    b.Property<int>("EndId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ConnectionId");

                    b.HasIndex("EndId");

                    b.ToTable("NodeMapping");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.SVGSnippet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.ToTable("Snippets");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.Warehouse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ConsumerId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<int>("NodeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ConsumerId");

                    b.ToTable("Warehouses");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.Connection", b =>
                {
                    b.HasOne("DanskLogistikAPI.Models.Node", "A")
                        .WithMany()
                        .HasForeignKey("AId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DanskLogistikAPI.Models.Node", "B")
                        .WithMany()
                        .HasForeignKey("BId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("A");

                    b.Navigation("B");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.Country", b =>
                {
                    b.HasOne("DanskLogistikAPI.Models.SVGSnippet", "DeJureOutline")
                        .WithMany()
                        .HasForeignKey("DeJureOutlineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DeJureOutline");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.Municipality", b =>
                {
                    b.HasOne("DanskLogistikAPI.Models.Country", "Controller")
                        .WithMany()
                        .HasForeignKey("ControllerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DanskLogistikAPI.Models.SVGSnippet", "Outline")
                        .WithMany()
                        .HasForeignKey("OutlineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DanskLogistikAPI.Models.Country", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Controller");

                    b.Navigation("Outline");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.Node", b =>
                {
                    b.HasOne("DanskLogistikAPI.Models.Municipality", "Location")
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Location");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.NodeMapping", b =>
                {
                    b.HasOne("DanskLogistikAPI.Models.Connection", "Connection")
                        .WithMany()
                        .HasForeignKey("ConnectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DanskLogistikAPI.Models.Node", "End")
                        .WithMany("Neighbors")
                        .HasForeignKey("EndId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Connection");

                    b.Navigation("End");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.Warehouse", b =>
                {
                    b.HasOne("DanskLogistikAPI.Models.Consumer", "Consumer")
                        .WithMany()
                        .HasForeignKey("ConsumerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Consumer");
                });

            modelBuilder.Entity("DanskLogistikAPI.Models.Node", b =>
                {
                    b.Navigation("Neighbors");
                });
#pragma warning restore 612, 618
        }
    }
}
