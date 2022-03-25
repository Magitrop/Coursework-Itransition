using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorCoursework.Data;

namespace RazorCoursework.Pages
{
    [Authorize]
    public class RemoveReviewModel : PageModel
    {
        public string CurrentReviewId { get; set; }
        public string CurrentReviewName { get; set; }

        public IActionResult OnGet(string id)
        {
            CurrentReviewId = id;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                var currentReview = context.Reviews
                    .Include(r => r.TagRelations)
                    .ThenInclude(r => r.Tag)
                    .FirstOrDefault(r => r.ReviewID == id);
                CurrentReviewName = currentReview.ReviewSubjectName;
                string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                bool checkUserOwnership = User.Identity.IsAuthenticated && (currentReview?.ReviewCreatorID == currentUserID || User.IsInRole("Admin"));
                if (!checkUserOwnership)
                    return RedirectToPage("/Home", new { user = User.Identity.Name, p = 1 });
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
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
                    var currentReview = reviews.FirstOrDefault(r => r.ReviewID == Request.Form["CurrentReviewId"].ToString());
                    if (currentReview != null)
                    {
                        context.Reviews.Remove(currentReview);
                        context.SaveChanges();
                    }
                }
            }

            return RedirectToPage("/Home", new { user = User.Identity.Name, p = 1 });
        }
    }
}
