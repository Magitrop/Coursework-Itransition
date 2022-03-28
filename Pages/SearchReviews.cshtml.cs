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
        public List<ReviewsListWithHeader> reviews { get; set; } = 
            new()
            {
                new(new(), string.Empty),
            };
        public int reviewsPerPage { get; set; } = 10;
        public string currentTag { get; set; }
        public int currentPage { get; set; }
        public int pagesCount { get; set; }

        public void OnGet(string tag, int p)
        {
            currentTag = tag ?? string.Empty;
            if (IsPageCorrect(p))
                LoadReviews();
        }

        private bool IsPageCorrect(int p) => (currentPage = p) >= 1;

        private void LoadReviews()
        {
            if (currentTag != string.Empty)
                LoadReviewsWithTag();
            else LoadAllReviews();
        }

        private void LoadReviewsWithTag()
        {
            using var context = AppContentDbContext.Create();
            var all = GetRelations(context);
            TakeReviewsWithRelations(all);
            pagesCount = (int)Math.Ceiling(all.Count() / (double)reviewsPerPage);
        }

        private IQueryable<UserReviewAndTagRelation> GetRelations(AppContentDbContext context)
        {
            return context.ReviewAndTagRelations
                .Include(r => r.Review)
                .ThenInclude(r => r.TagRelations)
                .ThenInclude(t => t.Tag)
                .Where(r => r.Tag.TagName == currentTag);
        }

        private void TakeReviewsWithRelations(IQueryable<UserReviewAndTagRelation> allReviews)
        {
            reviews[0].list = allReviews
                .Select(r => r.Review)
                .OrderByDescending(t => t.CreationDate)
                .Skip((currentPage - 1) * reviewsPerPage)
                .Take(reviewsPerPage)
                .ToList();
        }

        private void TakeAllReviews(AppContentDbContext context)
        {
            reviews[0].list =
                context.Reviews
                .Include(r => r.TagRelations)
                .ThenInclude(r => r.Tag)
                .OrderByDescending(r => r.CreationDate)
                .Skip((currentPage - 1) * reviewsPerPage)
                .Take(reviewsPerPage)
                .ToList();
        }

        private void LoadAllReviews()
        {
            using var context = AppContentDbContext.Create();
            TakeAllReviews(context);
            CalculatePagesCount(context);
        }

        private void CalculatePagesCount(AppContentDbContext context)
        {
            double reviewsDividedByPages = context.Reviews.Count() / (double)reviewsPerPage;
            pagesCount = (int)Math.Ceiling(reviewsDividedByPages);
        }

        public bool AlreadyLikedReview(string reviewID)
        {
            using var context = AppContentDbContext.Create();
            string currentUserID = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            return context.ReviewLikes.Any(like =>
                like.UserID == currentUserID &&
                like.ReviewID == reviewID);
        }

        public int GetLikesCount(string reviewID)
        {
            using var context = AppContentDbContext.Create();
            return context.ReviewLikes.Where(like => like.ReviewID == reviewID).Count();
        }

        public int GetCreatorLikesCount(string userID)
        {
            using var context = AppContentDbContext.Create();
            var creatorReviews = context.Reviews.Where(r => r.ReviewCreatorID == userID);
            return context.ReviewLikes.Where(like => creatorReviews.Any(r => r.ReviewID == like.ReviewID)).Count();
        }
    }
}
