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
using Microsoft.EntityFrameworkCore.Query;
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
            using var context = AppContentDbContext.Create();
            var currentReview = GetReviewByID(context, id);
            CurrentReviewId = id;
            CurrentReviewName = currentReview.ReviewSubjectName;
            return CheckUserOwnership(currentReview);
        }

        private Review GetReviewByID(AppContentDbContext context, string id) => 
            context.Reviews
                .Include(r => r.TagRelations)
                .ThenInclude(r => r.Tag)
                .FirstOrDefault(r => r.ReviewID == id);

        private IActionResult CheckUserOwnership(Review currentReview)
        {
            string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!User.Identity.IsAuthenticated &&
                (currentReview?.ReviewCreatorID == currentUserID || User.IsInRole("Admin")))
                return RedirectToPage("/Home", new { user = User.Identity.Name, p = 1 });
            return Page();
        }

        public IActionResult OnPost()
        {
            using var context = AppContentDbContext.Create();
            if (ModelState.IsValid)
                RemoveReview(context, Request.Form["CurrentReviewId"].ToString());
            return RedirectToPage("/Home", new { user = User.Identity.Name, p = 1 });
        }

        private void RemoveReview(AppContentDbContext context, string id)
        {
            var currentReview = context.Reviews.FirstOrDefault(r => r.ReviewID == id);
            if (currentReview != null)
            {
                context.Reviews.Remove(currentReview);
                context.SaveChanges();
            }
        }
    }
}
