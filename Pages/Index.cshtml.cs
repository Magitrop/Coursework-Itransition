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
        public class TagsWithQuantity
        {
            public string TagName { get; set; }
            public int TagQuantity { get; set; }

            public TagsWithQuantity(string tagName, int tagQuantity)
            {
                TagQuantity = tagQuantity;
                TagName = tagName;
            }
        }

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public List<ReviewsListWithHeader> reviews { get; set; } = 
            new()
            {
                new(new(), "RecentReviews"),
                new(new(), "TopRatedReviews")
            };
        public List<TagsWithQuantity> tags { get; set; } = new();
        public int showReviewsCount { get; set; } = 5;

        public void OnGet()
        {
            using var context = AppContentDbContext.Create();
            LoadRecentReviews(context);
            LoadTopRatedReviews(context);
            LoadTags(context);
        }

        private void LoadRecentReviews(AppContentDbContext context)
        {
            reviews[0].list = (from review in context.Reviews.Include(r => r.TagRelations).ThenInclude(r => r.Tag)
                        orderby review.CreationDate descending
                        select review)
                        .Take(showReviewsCount)
                        .ToList();
        }

        private void LoadTopRatedReviews(AppContentDbContext context)
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

        private void LoadTags(AppContentDbContext context)
        {
            var groups =
                    (from tag in context.ReviewAndTagRelations
                     group tag by tag.Tag.TagName into g
                     orderby g.Count() descending
                     select new { Key = g.Key, Count = g.Count() });
            foreach (var tag in groups)
                tags.Add(new TagsWithQuantity(tag.Key, tag.Count));
        }

        public int GetCreatorLikesCount(string userID)
        {
            using var context = AppContentDbContext.Create();
            var creatorReviews = context.Reviews.Where(r => r.ReviewCreatorID == userID);
            return context.ReviewLikes.Where(like => creatorReviews.Any(r => r.ReviewID == like.ReviewID)).Count();
        }
    }
}
