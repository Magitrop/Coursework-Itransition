using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using RazorCoursework.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorCoursework.Data;

namespace RazorCoursework.Pages
{
    public class SearchReviewsModel : PageModel
    {
        public List<Review> reviews { get; set; }
        public int reviewsPerPage { get; set; } = 4;
        public string currentTag { get; set; }
        public int currentPage { get; set; }
        public int pagesCount { get; set; }

        public void OnGet(string tag, int p)
        {
            currentTag = tag;
            currentPage = p;
            if (p < 1)
            {
                reviews = new List<Review>();
                return;
            }

            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                reviews =
                    context.ReviewAndTagRelations
                    .Include(r => r.Review)
                    .ThenInclude(r => r.TagRelations)
                    .ThenInclude(t => t.Tag)
                    .Where(r => r.Tag.TagName == tag)
                    .Select(r => r.Review)
                    .OrderByDescending(t => t.CreationDate)
                    .Skip((p - 1) * reviewsPerPage)
                    .Take(reviewsPerPage)
                    .ToList();

                var all = context.ReviewAndTagRelations
                    .Where(r => r.Tag.TagName == tag);
                double reviewsDividedByPages = all.Count() / (double)reviewsPerPage;
                pagesCount = (int)Math.Ceiling(reviewsDividedByPages);
            }
        }
    }
}
