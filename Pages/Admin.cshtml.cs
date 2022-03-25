using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorCoursework.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorCoursework.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminModel : PageModel
    {
        public List<IdentityUser> users;

        public async Task<IActionResult> OnGet()
        {
            using (var context = new ApplicationDbContext(
                  new DbContextOptionsBuilder<ApplicationDbContext>()
                  .UseSqlServer(Startup.Connection)
                  .Options))
            {
                users = await context.Users.ToListAsync();
            }

            return Page();
        }

        public int GetCreatorLikesCount(string userID)
        {
            int result = 0;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                var creatorReviews = context.Reviews.Where(r => r.ReviewCreatorID == userID);
                result = context.ReviewLikes.Where(like => creatorReviews.Any(r => r.ReviewID == like.ReviewID)).Count();
            }
            return result;
        }

        public string GetRole(string userID)
        {
            string result;
            using (var context = new ApplicationDbContext(
                   new DbContextOptionsBuilder<ApplicationDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                var roleId = context.UserRoles.FirstOrDefault(r => r.UserId == userID)?.RoleId;
                result = GetRoleScreenName(context.Roles.FirstOrDefault(r => r.Id == roleId)?.Name);
            }
            return result;
        }

        public string GetRoleScreenName(string roleRawName) => 
            roleRawName switch
            {
                "Admin" => "Администратор",
                _ => "Пользователь"
            };
    }
}
