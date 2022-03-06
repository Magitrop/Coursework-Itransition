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

        public async Task<IActionResult> OnPostLike()
        {
            MemoryStream stream = new MemoryStream();
            await Request.Body.CopyToAsync(stream);
            stream.Position = 0;
            string reviewID = string.Empty;
            bool alreadyLiked = false;
            using (StreamReader reader = new StreamReader(stream))
            {
                reviewID = await reader.ReadToEndAsync();
                using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
                {
                    var reviews = context.Reviews
                        .Include(r => r.Likes)
                        .Include(r => r.Ratings)
                        .Include(r => r.TagRelations)
                        .ThenInclude(r => r.Tag);
                    var currentReview = await reviews.FirstOrDefaultAsync(r => r.ReviewID == reviewID);
                    if (currentReview != null)
                    {
                        string currentUserID = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                        Like like = await context.ReviewLikes
                            .FirstOrDefaultAsync(l =>
                            l.ReviewID == currentReview.ReviewID &&
                            l.UserID == currentUserID);
                        if (alreadyLiked = like == null)
                        {
                            like = new Like()
                            {
                                Review = currentReview,
                                UserID = currentUserID
                            };
                            await context.ReviewLikes.AddAsync(like);
                        }
                        else
                            context.ReviewLikes.Remove(like);
                        await context.SaveChangesAsync();
                    }
                }
            }
            return new JsonResult(GetLikesCount(reviewID).ToString() + ';' + (!alreadyLiked).ToString());
        }

        public bool AlreadyLikedReview(string reviewID)
        {
            bool result = true;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                string currentuserID = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                result = context.ReviewLikes.Any(like =>
                    like.UserID == currentuserID &&
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
    }
}
