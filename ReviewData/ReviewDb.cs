using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewData
{
    public class ReviewDb : DbContext
    {
        public virtual DbSet<Review> Reviews { get; set; }

        public virtual DbSet<Purchase> Purchases { get; set; }

        public virtual DbSet<Customer> Customers { get; set; }

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

            builder.Entity<Customer>()
                .Property(c => c.CustomerId)
                // key is always provided
                .ValueGeneratedNever();

            builder.Entity<Customer>()
                .HasData(
                new Customer
                {
                    CustomerId = 1,
                    CustomerAuthId = "a64c9beb-534a-4b40-a9be-58ed21597cd0",
                    CustomerName = "Chris Burrell"
                },
                new Customer
                {
                    CustomerId = 2,
                    CustomerAuthId = "8e689e3c-24b1-400c-a8ad-7435c4fd15b5",
                    CustomerName = "Paul Mitchell"
                },
                new Customer
                {
                    CustomerId = 3,
                    CustomerAuthId = "94d6c9b0-b3c8-4ad6-96ed-c7ab43d6dd23",
                    CustomerName = "Jack Ferguson"
                },
                new Customer
                {
                    CustomerId = 4,
                    CustomerAuthId = "0313a3ca-e9d0-43c3-a580-ab25c6b224d8",
                    CustomerName = "Carter Ridgeway"
                },
                new Customer
                {
                    CustomerId = 5,
                    CustomerAuthId = "8de93d90-7e62-40e9-8032-602f835ee8ee",
                    CustomerName = "Karl Hall"
                });

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
                    Rating = 3,
                    ReviewText = "Good",
                    TimeStamp = new DateTime(),
                    Visible = true
                });
        }
    }
}