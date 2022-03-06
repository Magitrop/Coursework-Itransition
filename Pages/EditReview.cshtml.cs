using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorCoursework.Data;

namespace RazorCoursework.Pages
{
    [Authorize]
    public class EditReviewModel : PageModel
    {
        public string CurrentReviewId { get; set; }
        public string ReviewSubjectName { get; set; }
        public string ReviewSubjectGenre { get; set; }
        public string ReviewText { get; set; }
        public string ReviewTags { get; set; }

        [BindProperty] public InputModel Input { get; set; }

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
                if (!User.Identity.IsAuthenticated || currentReview?.ReviewCreatorName != User.Identity.Name)
                    return RedirectToPage("/Home", new { user = User.Identity.Name, p = 1 });
                else
                {
                    ReviewSubjectName = currentReview.ReviewSubjectName;
                    ReviewSubjectGenre = currentReview.ReviewSubjectGenre;
                    ReviewText = currentReview.ReviewText;
                    ReviewTags = string.Join(' ', currentReview.TagRelations.Select(r => r.Tag.TagName));
                }
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
                    context.Reviews.Remove(currentReview);
                    var newReview = new Review()
                    {
                        ReviewCreatorID = currentReview.ReviewCreatorID,
                        ReviewCreatorName = currentReview.ReviewCreatorName,
                        ReviewText = Input.ReviewText,
                        ReviewSubjectGenre = Input.ReviewSubjectGenre,
                        ReviewSubjectName = Input.ReviewSubjectName,
                        CreationDate = currentReview.CreationDate,
                        TagRelations = new List<UserReviewAndTagRelation>()
                    };
                    context.Reviews.Add(newReview);

                    foreach (var t in from t in Input.Tags.Split(',')
                                        where t.Length > 0
                                        select t)
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
