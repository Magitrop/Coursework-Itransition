using RazorCoursework.Data;
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
        public DbSet<Rating> ReviewRatings { get; set; }
        public DbSet<Like> ReviewLikes { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }

        public AppContentDbContext(DbContextOptions<AppContentDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Review>()
                .HasMany(p => p.TagRelations)
                .WithOne(b => b.Review)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade)
                .HasForeignKey(prop => prop.ReviewID);

            modelBuilder.Entity<Rating>()
                .HasOne(p => p.Review)
                .WithMany(b => b.Ratings)
                .OnDelete(DeleteBehavior.Cascade)
                .HasForeignKey(prop => prop.ReviewID);

            modelBuilder.Entity<Like>()
                .HasOne(p => p.Review)
                .WithMany(b => b.Likes)
                .OnDelete(DeleteBehavior.Cascade)
                .HasForeignKey(prop => prop.ReviewID);

            modelBuilder.Entity<Tag>()
                .HasMany(p => p.ReviewRelations)
                .WithOne(b => b.Tag)
                .IsRequired()
                .HasForeignKey(prop => prop.TagID);

            base.OnModelCreating(modelBuilder);
        }

        public static void CreateUserPreferences(string userID)
        {
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                UserPreferences preferences = new UserPreferences()
                {
                    UserID = userID,
                    IsDarkTheme = false,
                    IsEnglishVersion = false
                };
                context.UserPreferences.Add(preferences);
                context.SaveChanges();
            }
        }

        public static UserPreferences GetUserPreferences(string userID)
        {
            if (userID == null)
                return new UserPreferences();

            UserPreferences preferences;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                var found = context.UserPreferences.FirstOrDefault(p => p.UserID == userID);
                if (found == null)
                {
                    CreateUserPreferences(userID);
                    preferences = context.UserPreferences.FirstOrDefault(p => p.UserID == userID);
                }
                else
                {
                    preferences = new UserPreferences()
                    {
                        PreferenceID = found.PreferenceID,
                        UserID = found.UserID,
                        IsDarkTheme = found.IsDarkTheme,
                        IsEnglishVersion = found.IsEnglishVersion
                    };
                }
            }
            return preferences;
        }
    }
}
