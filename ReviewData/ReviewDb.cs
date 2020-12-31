using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewData
{
    public class ReviewDb : DbContext
    {
        public DbSet<Review> Reviews { get; set; }

        public DbSet<Purchase> Purchases { get; set; }

        public DbSet<Customer> Customers { get; set; }

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
                    CustomerAuthId = "b45727f0-bf10-40dc-a687-f5cd025630f2",
                    CustomerName = "Chris Burrell"
                },
                new Customer
                {
                    CustomerId = 2,
                    CustomerAuthId = "286fa26e-ae5f-4c5a-b89d-7301fb247d78",
                    CustomerName = "Paul Mitchell"
                },
                new Customer
                {
                    CustomerId = 3,
                    CustomerAuthId = "b477a6e4-6607-43c9-8ea0-c2367a5b0360",
                    CustomerName = "Jack Ferguson"
                },
                new Customer
                {
                    CustomerId = 4,
                    CustomerAuthId = "9fe723cf-7ac2-4b51-a79a-9e5813fb306a",
                    CustomerName = "Carter Ridgeway"
                },
                new Customer
                {
                    CustomerId = 5,
                    CustomerAuthId = "727c783f-ede7-4e53-a365-f8c830e327f4",
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