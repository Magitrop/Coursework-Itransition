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
using Dropbox.Api;

namespace RazorCoursework.Pages
{
    public class ReviewsListWithHeader
    {
        public List<Review> list;
        public string header;

        public ReviewsListWithHeader(List<Review> _list, string _header)
        {
            list = _list;
            header = _header;
        }
    }

    public class HomeModel : PageModel
    {
        public List<ReviewsListWithHeader> reviews { get; set; } = 
            new()
            {
                new(new(), string.Empty),
            };
        public string userName { get; set; }
        public int reviewsPerPage { get; set; } = 10;
        public string currentTag { get; set; }
        public int currentPage { get; set; }
        public int pagesCount { get; set; }

        public ApplicationDbContext applicationContext;
        public AppContentDbContext contentContext;

        public HomeModel(ApplicationDbContext _applicationContext, AppContentDbContext _contentContext)
        {
            applicationContext = _applicationContext;
            contentContext = _contentContext;
        }

        public void OnGet(string user, int p)
        {
            userName = user;
            if (IsPageCorrect(p))
                LoadReviews();
        }

        private bool IsPageCorrect(int p) => (currentPage = p) >= 1;

        private void LoadReviews()
        {
            GetOwnReviews();
            CalculatePagesCount();
            TakeReviewsForPage();
        }

        private void GetOwnReviews()
        {
            string creatorID = applicationContext.Users.FirstOrDefault(u => u.UserName == userName)?.Id ?? string.Empty;
            reviews[0].list = (from t in contentContext.Reviews.Include(r => r.TagRelations).ThenInclude(r => r.Tag)
                               where t.ReviewCreatorID == creatorID
                               orderby t.CreationDate descending
                               select t).ToList();
        }

        private void CalculatePagesCount()
        {
            double reviewsDividedByPages = reviews[0].list.Count() / (double)reviewsPerPage;
            pagesCount = (int)Math.Ceiling(reviewsDividedByPages);
        }

        private void TakeReviewsForPage()
        {
            reviews[0].list = reviews[0].list
                .Skip((currentPage - 1) * reviewsPerPage)
                .Take(reviewsPerPage)
                .ToList();
        }

        public int GetCreatorLikesCount(string userID)
        {
            var creatorReviews = contentContext.Reviews.Where(r => r.ReviewCreatorID == userID);
            return contentContext.ReviewLikes.Where(like => creatorReviews.Any(r => r.ReviewID == like.ReviewID)).Count();
        }

        public IActionResult OnPostUserPreferences()
        {
            using var context = AppContentDbContext.Create();
            UserPreferences preferences = GetPreferencesOrDefault(context);
            context.SaveChanges();
            return new JsonResult(preferences.IsDarkTheme + ";" + preferences.IsEnglishVersion);
        }

        private UserPreferences GetPreferencesOrDefault(AppContentDbContext context)
        {
            string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (User.Identity.IsAuthenticated)
                return GetPreferences(context, currentUserID);
            else return new UserPreferences();
        }

        private UserPreferences GetPreferences(AppContentDbContext context, string currentUserID)
        {
            var preferences = context.UserPreferences.FirstOrDefault(p => p.UserID == currentUserID);
            if (preferences == null)
                context.UserPreferences.Add(preferences = new UserPreferences()
                {
                    UserID = currentUserID
                });
            return preferences;
        }

        public IActionResult OnPostSwitchTheme()
        {
            using var context = AppContentDbContext.Create();
            UserPreferences preferences = GetPreferencesOrDefault(context);
            preferences.IsDarkTheme = !preferences.IsDarkTheme;
            context.SaveChanges();
            return new JsonResult(preferences.IsDarkTheme);
        }

        public IActionResult OnPostSwitchLanguage()
        {
            using var context = AppContentDbContext.Create();
            UserPreferences preferences = GetPreferencesOrDefault(context);
            LocService.isEnglishVersion = preferences.IsEnglishVersion = !preferences.IsEnglishVersion;
            context.SaveChanges();
            return new JsonResult(LocService.isEnglishVersion);
        }

        public IActionResult OnPostGetLanguage()
        {
            using var context = AppContentDbContext.Create();
            UserPreferences preferences = GetPreferencesOrDefault(context);
            return new JsonResult(LocService.isEnglishVersion = preferences.IsEnglishVersion);
        }
    }
}
