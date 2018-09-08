﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tadmor.Services.Data;

namespace Tadmor.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.0-rtm-30799");

            modelBuilder.Entity("Tadmor.Services.Twitter.TwitterMedia", b =>
                {
                    b.Property<ulong>("TweetId");

                    b.Property<ulong>("MediaId");

                    b.Property<string>("StatusText");

                    b.Property<string>("Url");

                    b.Property<string>("Username");

                    b.HasKey("TweetId", "MediaId");

                    b.ToTable("TwitterMedia");
                });
#pragma warning restore 612, 618
        }
    }
}
