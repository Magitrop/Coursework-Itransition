using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using RazorCoursework.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace RazorCoursework.Pages
{
    public class AllReviewsModel : PageModel
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
                    context.Reviews
                    .Include(r => r.TagRelations)
                    .ThenInclude(r => r.Tag)
                    .OrderByDescending(t => t.CreationDate)
                    .Skip((p - 1) * reviewsPerPage)
                    .Take(reviewsPerPage)
                    .ToList();

                double reviewsDividedByPages = context.ReviewAndTagRelations.Count() / reviewsPerPage;
                pagesCount = (int)Math.Ceiling(reviewsDividedByPages);
            }
        }
    }
}
