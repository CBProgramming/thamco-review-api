using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewData
{
    public class ReviewDb : DbContext
    {
        public DbSet<Review> Reviews { get; set; }

        public ReviewDb(DbContextOptions<ReviewDb> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Review>()
                   .HasKey(r => new { r.CustomerId, r.ProductId });
        }
    }
}