﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tadmor.Services.Data;

namespace Tadmor.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20191201202105_Babies")]
    partial class Babies
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.0.0");

            modelBuilder.Entity("Tadmor.Services.Marriage.Babies.Baby", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("MarriedCoupleId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("MarriedCoupleId");

                    b.ToTable("Babies");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Baby");
                });

            modelBuilder.Entity("Tadmor.Services.Marriage.MarriedCouple", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("KissCooldown")
                        .HasColumnType("TEXT");

                    b.Property<float>("Kisses")
                        .HasColumnName("FloatKisses")
                        .HasColumnType("REAL");

                    b.Property<int>("KissesLegacy")
                        .HasColumnName("Kisses")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastKissed")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("Partner1Id")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Partner2Id")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Partner1Id", "Partner2Id", "GuildId")
                        .IsUnique();

                    b.ToTable("MarriedCouples");
                });

            modelBuilder.Entity("Tadmor.Services.Twitter.TwitterMedia", b =>
                {
                    b.Property<ulong>("TweetId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("MediaId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Username")
                        .HasColumnType("TEXT");

                    b.Property<string>("StatusText")
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.HasKey("TweetId", "MediaId", "Username");

                    b.ToTable("TwitterMedia");
                });

            modelBuilder.Entity("Tadmor.Services.Marriage.Babies.EvilBaby", b =>
                {
                    b.HasBaseType("Tadmor.Services.Marriage.Babies.Baby");

                    b.HasDiscriminator().HasValue("EvilBaby");
                });

            modelBuilder.Entity("Tadmor.Services.Marriage.Babies.LoveBaby", b =>
                {
                    b.HasBaseType("Tadmor.Services.Marriage.Babies.Baby");

                    b.HasDiscriminator().HasValue("LoveBaby");
                });

            modelBuilder.Entity("Tadmor.Services.Marriage.Babies.NormalBaby", b =>
                {
                    b.HasBaseType("Tadmor.Services.Marriage.Babies.Baby");

                    b.HasDiscriminator().HasValue("NormalBaby");
                });

            modelBuilder.Entity("Tadmor.Services.Marriage.Babies.Baby", b =>
                {
                    b.HasOne("Tadmor.Services.Marriage.MarriedCouple", null)
                        .WithMany("Babies")
                        .HasForeignKey("MarriedCoupleId");
                });
#pragma warning restore 612, 618
        }
    }
}