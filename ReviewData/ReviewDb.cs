﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewData
{
    public class ReviewDb : DbContext
    {
        public DbSet<Review> Reviews { get; set; }

        public DbSet<Purchase> Purchases { get; set; }

        public ReviewDb(DbContextOptions<ReviewDb> options) : base(options)
        {
        }

        public ReviewDb()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("reviews");

            builder.Entity<Review>()
                   .HasKey(r => new { r.CustomerId, r.ProductId });

            builder.Entity<Purchase>()
                   .HasKey(p => new { p.CustomerId, p.ProductId });

            builder.Entity<Purchase>()
                .HasData(
                new Purchase
                {
                    CustomerId = 1,
                    ProductId = 1
                },
                new Purchase
                {
                    CustomerId = 1,
                    ProductId = 2
                },
                new Purchase
                {
                    CustomerId = 1,
                    ProductId = 3
                },
                new Purchase
                {
                    CustomerId = 2,
                    ProductId = 3
                },
                new Purchase
                {
                    CustomerId = 2,
                    ProductId = 4
                },
                new Purchase
                {
                    CustomerId = 3,
                    ProductId = 5
                }
            );

            builder.Entity<Review>()
                .HasData(
                new Review
                {
                    CustomerId = 1,
                    ProductId = 1,
                    CustomerName = "CustomerName",
                    Rating = 3,
                    ReviewText = "Good",
                    TimeStamp = new DateTime(),
                    Visible = true
                });
        }
    }
}