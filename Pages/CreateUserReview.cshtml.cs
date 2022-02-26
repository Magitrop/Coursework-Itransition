using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Coursework_Itransition.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorCoursework.Data;

namespace RazorCoursework.Pages
{
    public class CreateUserReviewModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Введите название обзора.")]
            [Display(Name = "Название")]
            public string ReviewSubjectName { get; set; }

            [Required(ErrorMessage = "Введите жанр.")]
            [Display(Name = "Жанр")]
            public string ReviewSubjectGenre { get; set; }

            [Required(ErrorMessage = "Введите текст обзора.")]
            [Display(Name = "Текст обзора")]
            public string ReviewText { get; set; }

            [Display(Name = "Теги (указываются через пробелы)")]
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
                        ReviewText = Input.ReviewText,
                        ReviewSubjectGenre = Input.ReviewSubjectGenre,
                        ReviewSubjectName = Input.ReviewSubjectName,
                        TagRelations = new List<UserReviewAndTagRelation>()
                    };
                    context.Reviews.Add(newReview);

                    foreach (var tag in from t in Input.Tags.Split(' ')
                                        where t.Length > 0
                                        select t)
                    {
                        Tag newTag = context.Tags.Include(t => t.ReviewRelations).FirstOrDefault(_t => _t.TagName == tag);
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

            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                foreach (var r in 
                    context.ReviewAndTagRelations
                    .Include(r => r.Review)
                    .Include(r => r.Tag))
                    System.Diagnostics.Debug.WriteLine(r.Review.ReviewSubjectName + ": " + r.Tag.TagName);
            }

            return Page();
        }
    }
}
