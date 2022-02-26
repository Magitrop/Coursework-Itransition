using Coursework_Itransition.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorCoursework.Data
{
    public class AppContentDbContext : DbContext
    {
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<UserReviewAndTagRelation> ReviewAndTagRelations { get; set; }

        public AppContentDbContext(DbContextOptions<AppContentDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Review>()
                .HasMany(p => p.TagRelations)
                .WithOne(b => b.Review)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade)
                .HasForeignKey(prop => prop.ReviewID);

            modelBuilder.Entity<Tag>()
                .HasMany(p => p.ReviewRelations)
                .WithOne(b => b.Tag)
                .IsRequired()
                .HasForeignKey(prop => prop.TagID);

            base.OnModelCreating(modelBuilder);
        }
    }
}
