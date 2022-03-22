using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using RazorCoursework.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace RazorCoursework.Pages
{
    public class SearchReviewsModel : PageModel
    {
        public List<Review> reviews { get; set; }
        public int reviewsPerPage { get; set; } = 10;
        public string currentSearchCondition { get; set; }
        public string currentTag { get; set; }
        public int currentPage { get; set; }
        public int pagesCount { get; set; }

        public void OnGet(string search, string tag, int p)
        {
            currentSearchCondition = search ?? string.Empty;
            if (currentSearchCondition == string.Empty)
                currentTag = tag ?? string.Empty;

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
            if (currentSearchCondition != string.Empty)
                LoadReviewsWithText();
            else if (currentTag != string.Empty)
                LoadReviewsWithTag();
            else LoadAllReviews();
        }

        private void LoadReviewsWithTag()
        {
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                var all = context.ReviewAndTagRelations
                    .Include(r => r.Review)
                    .ThenInclude(r => r.TagRelations)
                    .ThenInclude(t => t.Tag)
                    .Where(r => r.Tag.TagName == currentTag);

                reviews = all
                    .Select(r => r.Review)
                    .OrderByDescending(t => t.CreationDate)
                    .Skip((currentPage - 1) * reviewsPerPage)
                    .Take(reviewsPerPage)
                    .ToList();
                
                double reviewsDividedByPages = all.Count() / (double)reviewsPerPage;
                pagesCount = (int)Math.Ceiling(reviewsDividedByPages);
            }
        }

        private void LoadReviewsWithText()
        {
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                var all = context.ReviewAndTagRelations
                    .Include(r => r.Review)
                    .ThenInclude(r => r.TagRelations)
                    .ThenInclude(t => t.Tag)
                    .Select(r => r.Review)
                    .Where(r => EF.Functions.FreeText(r.ReviewText, currentSearchCondition));

                reviews = all
                    .OrderByDescending(t => t.CreationDate)
                    .Skip((currentPage - 1) * reviewsPerPage)
                    .Take(reviewsPerPage)
                    .ToList();

                double reviewsDividedByPages = all.Count() / (double)reviewsPerPage;
                pagesCount = (int)Math.Ceiling(reviewsDividedByPages);
            }
        }

        private void LoadAllReviews()
        {
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                reviews =
                    context.Reviews
                    .Include(r => r.TagRelations)
                    .ThenInclude(r => r.Tag)
                    .OrderByDescending(r => r.CreationDate)
                    .Skip((currentPage - 1) * reviewsPerPage)
                    .Take(reviewsPerPage)
                    .ToList();

                double reviewsDividedByPages = context.Reviews.Count() / (double)reviewsPerPage;
                pagesCount = (int)Math.Ceiling(reviewsDividedByPages);
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
                string currentUserID = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
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
    }
}
