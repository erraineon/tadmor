﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tadmor.Services.Data;

namespace Tadmor.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20200404161942_Add_Upvotes")]
    partial class Add_Upvotes
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.0.0");

            modelBuilder.Entity("Tadmor.Services.Reddit.Upvote", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UpvotesCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("GuildId", "UserId");

                    b.ToTable("Upvotes");
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
#pragma warning restore 612, 618
        }
    }
}