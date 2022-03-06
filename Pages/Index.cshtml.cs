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

        public List<Review> reviews { get; set; }
        public int recentReviewsCount { get; set; } = 4;

        public void OnGet()
        {
            LoadReviews();
        }

        private void LoadReviews()
        {
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                reviews = (from t in context.Reviews.Include(r => r.TagRelations).ThenInclude(r => r.Tag)
                           orderby t.CreationDate descending
                           select t)
                           .Take(recentReviewsCount)
                           .ToList();
            }
        }
    }
}
