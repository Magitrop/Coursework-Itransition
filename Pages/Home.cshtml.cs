using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using RazorCoursework.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;

namespace RazorCoursework.Pages
{
    public class HomeModel : PageModel
    {
        public List<Review> reviews { get; set; }
        public string userName { get; set; }
        public int reviewsPerPage { get; set; } = 10;
        public string currentTag { get; set; }
        public int currentPage { get; set; }
        public int pagesCount { get; set; }

        public void OnGet(string user, int p)
        {
            userName = user;
            if (IsPageCorrect(p))
                LoadReviews();
            else reviews = new List<Review>();
        }

        private bool IsPageCorrect(int p)
        {
            currentPage = p;
            return p >= 1;
        }

        private void LoadReviews()
        {
            string creatorID;
            using (var context = new ApplicationDbContext(
                   new DbContextOptionsBuilder<ApplicationDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                creatorID = context.Users.FirstOrDefault(u => u.UserName == userName)?.Id ?? string.Empty;
            }

            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                reviews = (from t in context.Reviews.Include(r => r.TagRelations).ThenInclude(r => r.Tag)
                             where t.ReviewCreatorID == creatorID
                             orderby t.CreationDate descending
                             select t).ToList();

                double reviewsDividedByPages = reviews.Count() / (double)reviewsPerPage;
                pagesCount = (int)Math.Ceiling(reviewsDividedByPages);

                reviews = reviews
                    .Skip((currentPage - 1) * reviewsPerPage)
                    .Take(reviewsPerPage)
                    .ToList();
            }
        }

        public bool AlreadyLikedReview(string reviewID)
        {
            bool result = true;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                result = context.ReviewLikes.Any(like =>
                    like.UserID == currentUserID &&
                    like.ReviewID == reviewID);
            }
            return result;
        }

        public int GetLikesCount(string reviewID)
        {
            int result = 0;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                result = context.ReviewLikes.Where(like => like.ReviewID == reviewID).Count();
            }
            return result;
        }

        public int GetCreatorLikesCount(string userID)
        {
            int result = 0;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                var creatorReviews = context.Reviews.Where(r => r.ReviewCreatorID == userID);
                result = context.ReviewLikes.Where(like => creatorReviews.Any(r => r.ReviewID == like.ReviewID)).Count();
            }
            return result;
        }

        public IActionResult OnPostUserPreferences()
        {
            UserPreferences preferences;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (User.Identity.IsAuthenticated)
                {
                    preferences = context.UserPreferences.FirstOrDefault(p => p.UserID == currentUserID);
                    if (preferences == null)
                    {
                        context.UserPreferences.Add(preferences = new UserPreferences()
                        {
                            UserID = currentUserID
                        });
                        context.SaveChanges();
                    }
                }
                else preferences = new UserPreferences();
            }
            return new JsonResult(preferences.IsDarkTheme + ";" + preferences.IsEnglishVersion);
        }

        public async Task<IActionResult> OnPostSwitchTheme()
        {
            UserPreferences preferences;
            bool isLightTheme;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (User.Identity.IsAuthenticated)
                {
                    preferences = await context.UserPreferences.FirstOrDefaultAsync(p => p.UserID == currentUserID);
                    if (preferences == null)
                        await context.UserPreferences.AddAsync(preferences = new UserPreferences()
                        {
                            UserID = currentUserID
                        });
                }
                else preferences = new UserPreferences();

                isLightTheme = preferences.IsDarkTheme = !preferences.IsDarkTheme;
                context.SaveChanges();
            }
            return new JsonResult(isLightTheme);
        }
    }
}
