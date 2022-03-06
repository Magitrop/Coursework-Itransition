using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorCoursework.Data;

namespace RazorCoursework.Pages
{
    public class ReviewCommentsModel : PageModel
    {
        public Review Review { get; set; }

        public IActionResult OnGet(string id)
        {
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                Review = context.Reviews
                    .Include(r => r.TagRelations)
                    .ThenInclude(r => r.Tag)
                    .FirstOrDefault(r => r.ReviewID == id);
            }

            return Page();
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
