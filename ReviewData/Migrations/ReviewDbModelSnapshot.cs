﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ReviewData;

namespace ReviewData.Migrations
{
    [DbContext(typeof(ReviewDb))]
    partial class ReviewDbModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("reviews")
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("ReviewData.Purchase", b =>
                {
                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.HasKey("CustomerId", "ProductId");

                    b.ToTable("Purchases");

                    b.HasData(
                        new
                        {
                            CustomerId = 1,
                            ProductId = 1
                        },
                        new
                        {
                            CustomerId = 1,
                            ProductId = 2
                        },
                        new
                        {
                            CustomerId = 1,
                            ProductId = 3
                        },
                        new
                        {
                            CustomerId = 2,
                            ProductId = 3
                        },
                        new
                        {
                            CustomerId = 2,
                            ProductId = 4
                        },
                        new
                        {
                            CustomerId = 3,
                            ProductId = 5
                        });
                });

            modelBuilder.Entity("ReviewData.Review", b =>
                {
                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Rating")
                        .HasColumnType("int");

                    b.Property<string>("ReviewText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Visible")
                        .HasColumnType("bit");

                    b.HasKey("CustomerId", "ProductId");

                    b.ToTable("Reviews");

                    b.HasData(
                        new
                        {
                            CustomerId = 1,
                            ProductId = 1,
                            CustomerName = "CustomerName",
                            Rating = 3,
                            ReviewText = "Good",
                            TimeStamp = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Visible = true
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
