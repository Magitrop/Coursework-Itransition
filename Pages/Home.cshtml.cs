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
using RazorCoursework.Data;

namespace RazorCoursework.Pages
{
    [Authorize]
    public class HomeModel : PageModel
    {
        public List<Review> myReviews { get; set; }

        public void OnGet()
        {
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                string creatorID = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                myReviews = (from t in context.Reviews.Include(r => r.TagRelations).ThenInclude(r => r.Tag)
                            where t.ReviewCreatorID == creatorID
                            orderby t.CreationDate descending
                            select t).ToList();
            }
        }
    }
}
