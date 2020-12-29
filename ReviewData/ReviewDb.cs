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
                    CustomerAuthId = "f756701c-4336-47b1-8317-a16e84bd0059",
                    CustomerName = "Chris Burrell"
                },
                new Customer
                {
                    CustomerId = 2,
                    CustomerAuthId = "07dc5dfc-9dad-408c-ba81-ff6a8dd3aec2",
                    CustomerName = "Paul Mitchell"
                },
                new Customer
                {
                    CustomerId = 3,
                    CustomerAuthId = "1e3998f7-4ca6-42e0-9c78-8cb030f65f47",
                    CustomerName = "Jack Ferguson"
                },
                new Customer
                {
                    CustomerId = 4,
                    CustomerAuthId = "bce3bb9c-5947-4265-8a7d-8588655bbabe",
                    CustomerName = "Carter Ridgeway"
                },
                new Customer
                {
                    CustomerId = 5,
                    CustomerAuthId = "fb9e3941-6830-4387-be15-eeac14848c01",
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