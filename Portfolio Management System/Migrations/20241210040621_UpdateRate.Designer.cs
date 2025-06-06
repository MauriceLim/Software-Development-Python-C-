﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MonteCarloSimulatorAPI.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MonteCarloSimulatorAPI.Migrations
{
    [DbContext(typeof(FinancialDbContext))]
    [Migration("20241210040621_UpdateRate")]
    partial class UpdateRate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Curve", b =>
                {
                    b.Property<int>("CurveID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CurveID"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("CurveID");

                    b.ToTable("Curves");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Derivative", b =>
                {
                    b.Property<int>("DerivativeID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("DerivativeID"));

                    b.Property<double?>("BarrierLevel")
                        .HasColumnType("double precision");

                    b.Property<string>("BarrierType")
                        .HasColumnType("text");

                    b.Property<DateTime>("ExpirationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool?>("IsCall")
                        .HasColumnType("boolean");

                    b.Property<double?>("Payout")
                        .HasColumnType("double precision");

                    b.Property<double>("StrikePrice")
                        .HasColumnType("double precision");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("UnderlyingID")
                        .HasColumnType("integer");

                    b.HasKey("DerivativeID");

                    b.HasIndex("UnderlyingID");

                    b.ToTable("Derivatives");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Exchange", b =>
                {
                    b.Property<int>("ExchangeID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ExchangeID"));

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ExchangeID");

                    b.ToTable("Exchanges");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Market", b =>
                {
                    b.Property<int>("MarketID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("MarketID"));

                    b.Property<int>("ExchangeID")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("MarketID");

                    b.HasIndex("ExchangeID");

                    b.ToTable("Markets");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Price", b =>
                {
                    b.Property<int>("PriceID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("PriceID"));

                    b.Property<double>("ClosePrice")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<double>("HighPrice")
                        .HasColumnType("double precision");

                    b.Property<double>("LowPrice")
                        .HasColumnType("double precision");

                    b.Property<double>("OpenPrice")
                        .HasColumnType("double precision");

                    b.Property<int>("UnderlyingID")
                        .HasColumnType("integer");

                    b.Property<long>("Volume")
                        .HasColumnType("bigint");

                    b.HasKey("PriceID");

                    b.HasIndex("UnderlyingID");

                    b.ToTable("Prices");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Rate", b =>
                {
                    b.Property<int>("RateID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("RateID"));

                    b.Property<int>("CurveID")
                        .HasColumnType("integer");

                    b.Property<double>("Tenor")
                        .HasColumnType("double precision");

                    b.Property<double>("Value")
                        .HasColumnType("double precision");

                    b.HasKey("RateID");

                    b.HasIndex("CurveID");

                    b.ToTable("Rates");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Trade", b =>
                {
                    b.Property<int>("TradeID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TradeID"));

                    b.Property<double?>("CurrentPrice")
                        .HasColumnType("double precision");

                    b.Property<double>("Delta")
                        .HasColumnType("double precision");

                    b.Property<int?>("DerivativeID")
                        .HasColumnType("integer");

                    b.Property<double>("Gamma")
                        .HasColumnType("double precision");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer");

                    b.Property<double>("Theta")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("TradeDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<double>("TradePrice")
                        .HasColumnType("double precision");

                    b.Property<int?>("UnderlyingID")
                        .HasColumnType("integer");

                    b.Property<double>("Vega")
                        .HasColumnType("double precision");

                    b.HasKey("TradeID");

                    b.HasIndex("DerivativeID");

                    b.HasIndex("UnderlyingID");

                    b.ToTable("Trades");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Underlying", b =>
                {
                    b.Property<int>("UnderlyingID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("UnderlyingID"));

                    b.Property<int>("MarketID")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("UnderlyingID");

                    b.HasIndex("MarketID");

                    b.ToTable("Underlyings");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Derivative", b =>
                {
                    b.HasOne("MonteCarloSimulatorAPI.DataModels.Underlying", "Underlying")
                        .WithMany("Derivatives")
                        .HasForeignKey("UnderlyingID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Underlying");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Market", b =>
                {
                    b.HasOne("MonteCarloSimulatorAPI.DataModels.Exchange", "Exchange")
                        .WithMany("Markets")
                        .HasForeignKey("ExchangeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Exchange");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Price", b =>
                {
                    b.HasOne("MonteCarloSimulatorAPI.DataModels.Underlying", "Underlying")
                        .WithMany("Prices")
                        .HasForeignKey("UnderlyingID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Underlying");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Rate", b =>
                {
                    b.HasOne("MonteCarloSimulatorAPI.DataModels.Curve", "Curve")
                        .WithMany("Rates")
                        .HasForeignKey("CurveID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Curve");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Trade", b =>
                {
                    b.HasOne("MonteCarloSimulatorAPI.DataModels.Derivative", "Derivative")
                        .WithMany("Trades")
                        .HasForeignKey("DerivativeID");

                    b.HasOne("MonteCarloSimulatorAPI.DataModels.Underlying", "Underlying")
                        .WithMany("Trades")
                        .HasForeignKey("UnderlyingID");

                    b.Navigation("Derivative");

                    b.Navigation("Underlying");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Underlying", b =>
                {
                    b.HasOne("MonteCarloSimulatorAPI.DataModels.Market", "Market")
                        .WithMany("Underlyings")
                        .HasForeignKey("MarketID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Market");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Curve", b =>
                {
                    b.Navigation("Rates");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Derivative", b =>
                {
                    b.Navigation("Trades");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Exchange", b =>
                {
                    b.Navigation("Markets");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Market", b =>
                {
                    b.Navigation("Underlyings");
                });

            modelBuilder.Entity("MonteCarloSimulatorAPI.DataModels.Underlying", b =>
                {
                    b.Navigation("Derivatives");

                    b.Navigation("Prices");

                    b.Navigation("Trades");
                });
#pragma warning restore 612, 618
        }
    }
}
