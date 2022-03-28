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
            using var context = ApplicationDbContext.Create();
            users = await context.Users.ToListAsync();
            return Page();
        }

        public int GetCreatorLikesCount(string userID)
        {
            using var context = AppContentDbContext.Create();
            var creatorReviews = context.Reviews.Where(r => r.ReviewCreatorID == userID);
            return context.ReviewLikes.Where(like => creatorReviews.Any(r => r.ReviewID == like.ReviewID)).Count();
        }

        public string GetRole(string userID)
        {
            using var context = ApplicationDbContext.Create();
            var roleId = context.UserRoles.FirstOrDefault(r => r.UserId == userID)?.RoleId;
            return GetRoleScreenName(context.Roles.FirstOrDefault(r => r.Id == roleId)?.Name);
        }

        public string GetRoleScreenName(string roleRawName) => 
            roleRawName switch
            {
                "Admin" => "Администратор",
                _ => "Пользователь"
            };
    }
}
