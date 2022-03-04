using System;
using System.Collections.Generic;
using System.Linq;
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
                if (Review?.ReviewCreatorName != User.Identity.Name)
                    return RedirectToPage("/Home", new { user = User.Identity.Name, p = 1 });
            }

            return Page();
        }
    }
}
