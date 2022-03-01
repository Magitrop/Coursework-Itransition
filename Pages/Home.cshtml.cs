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

namespace RazorCoursework.Pages
{
    public class HomeModel : PageModel
    {
        public List<Review> reviews { get; set; }
        public string userName { get; set; }
        public int reviewsPerPage { get; set; } = 4;
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
    }
}
