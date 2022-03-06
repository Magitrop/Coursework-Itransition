using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using RazorCoursework.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace RazorCoursework.Pages
{
    [Authorize]
    public class CreateUserReviewModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "������� �������� ������.")]
            [Display(Name = "��������")]
            public string ReviewSubjectName { get; set; }

            [Required(ErrorMessage = "������� ����.")]
            [Display(Name = "����")]
            public string ReviewSubjectGenre { get; set; }

            [Required(ErrorMessage = "������� ����� ������.")]
            [Display(Name = "����� ������")]
            public string ReviewText { get; set; }

            [Display(Name = "���� (����������� ����� �������)")]
            public string Tags { get; set; }
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
                    var newReview = new Review()
                    {
                        ReviewCreatorID = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value,
                        ReviewCreatorName = User.Identity.Name,
                        ReviewText = Input.ReviewText,
                        ReviewSubjectGenre = Input.ReviewSubjectGenre,
                        ReviewSubjectName = Input.ReviewSubjectName,
                        CreationDate = DateTime.Now,
                        TagRelations = new List<UserReviewAndTagRelation>()
                    };
                    context.Reviews.Add(newReview);

                    foreach (var t in from t in Input.Tags.Split(',')
                                        where t.Length > 0
                                        select t.Trim())
                    {
                        var tag = Regex.Replace(t, @"[ ]{2,}", " ");

                        Tag newTag = context.Tags
                            .Include(t => t.ReviewRelations)
                            .FirstOrDefault(_t => _t.TagName == tag);
                        if (newTag == null)
                        {
                            newTag = new Tag()
                            {
                                TagName = tag,
                                ReviewRelations = new List<UserReviewAndTagRelation>()
                            };
                            context.Tags.Add(newTag);
                        }

                        var rel = new UserReviewAndTagRelation()
                        {
                            Tag = newTag,
                            Review = newReview
                        };
                        newTag.ReviewRelations.Add(rel);
                        newReview.TagRelations.Add(rel);
                        context.ReviewAndTagRelations.Add(rel);
                    }

                    context.SaveChanges();
                }
            }

            return RedirectToPage("/Home", new { user = User.Identity.Name, p = 1 });
        }
    }
}
