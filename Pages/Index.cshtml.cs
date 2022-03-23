using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorCoursework.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorCoursework.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public List<ReviewsListWithHeader> reviews { get; set; }
        public int showReviewsCount { get; set; } = 5;

        public void OnGet()
        {
            reviews = new List<ReviewsListWithHeader>()
            {
                new ReviewsListWithHeader(new List<Review>(), "Последние обзоры пользователей"),
                new ReviewsListWithHeader(new List<Review>(), "Обзоры с самыми большими оценками")
            };
            LoadRecentReviews();
            LoadTopRatedReviews();
        }

        private void LoadRecentReviews()
        {
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                reviews[0].list = (from review in context.Reviews.Include(r => r.TagRelations).ThenInclude(r => r.Tag)
                           orderby review.CreationDate descending
                           select review)
                           .Take(showReviewsCount)
                           .ToList();
            }
        }

        private void LoadTopRatedReviews()
        {
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                var groups =
                        (from like in context.ReviewLikes
                         group like by like.ReviewID into g
                         orderby g.Count() descending
                         select g.Key)
                        .Take(showReviewsCount);
                reviews[1].list = context.Reviews
                    .Include(r => r.TagRelations)
                    .ThenInclude(t => t.Tag)
                    .Where(r => groups.Any(g => g == r.ReviewID)).ToList();
            }
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
