using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dropbox.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorCoursework.Data;

namespace RazorCoursework.Pages
{
    [Authorize]
    public class EditReviewModel : PageModel
    {
        private readonly IWebHostEnvironment _appEnvironment;
        public EditReviewModel(IWebHostEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }

        public string CurrentReviewId { get; set; }
        public string ReviewSubjectName { get; set; }
        public string ReviewSubjectGenre { get; set; }
        public string ReviewText { get; set; }
        public string ReviewTags { get; set; }
        public List<(int index, string link)> ReviewPictureLinks { get; set; } = new List<(int index, string link)>();

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

            [Display(Name = "Теги (указываются через запятую)")]
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
                    ReviewTags = string.Join(',', currentReview.TagRelations.Select(r => r.Tag.TagName));

                    int index = 0;
                    foreach (var link in currentReview.AttachedPictureLinks.Split(';').Where(p => p.Length > 0))
                        ReviewPictureLinks.Add((++index, link));
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var pictureLinks = await GetPictureLinks();

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
                        AttachedPictureLinks = pictureLinks,
                        TagRelations = new List<UserReviewAndTagRelation>()
                    };
                    context.Reviews.Add(newReview);

                    if (Input.Tags?.Length > 0)
                    {
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
                    }

                    context.SaveChanges();
                }
            }

            return RedirectToPage("/Home", new { user = User.Identity.Name, p = 1 });
        }

        private async Task<string> GetPictureLinks()
        {
            string pictureLinks = string.Empty;
            int filesCount;
            if (int.TryParse(Request.Form["FilesCount"], out filesCount))
                for (int i = 1; i <= filesCount; i++)
                    if (Request.Form.ContainsKey("File-" + i))
                        pictureLinks += Request.Form["File-" + i] + ";";

            if (Request.Form.Files == null || Request.Form.Files.Count == 0)
                return pictureLinks;

            var tempDirectory = _appEnvironment.WebRootPath + "/files/";
            if (!Directory.Exists(tempDirectory))
                Directory.CreateDirectory(tempDirectory);

            using (var dbx = new DropboxClient("yvwtJ6G2tG0AAAAAAAAAAdQTdoQZAz8BXbFqFTSxCWF31KZNiRGYuqHThF_uAYLA"))
            {
                foreach (var file in Request.Form.Files)
                {
                    string filepath = string.Empty;
                    if (file.Length > 0)
                    {
                        if (file.Length <= 4096 * 1024)
                        {
                            using (var stream = new FileStream(
                                tempDirectory + Guid.NewGuid() + "_" + file.FileName, FileMode.CreateNew))
                            {
                                file.CopyTo(stream);
                                filepath = stream.Name;
                            }
                            using (var fileStream = System.IO.File.Open(filepath, FileMode.Open))
                            {
                                var uploaded = await dbx.Files.UploadAsync(
                                    "/" + Guid.NewGuid() + "_" + file.FileName,
                                    body: fileStream);
                                pictureLinks +=
                                    (await dbx.Sharing.CreateSharedLinkWithSettingsAsync(uploaded.PathLower))
                                    .Url.Replace("dl=0", "raw=1") + ";";
                            }
                            System.IO.File.Delete(filepath);
                        }
                        else
                            ModelState.AddModelError(string.Empty, "Вес загружаемого изображения не должен превышать 4 МБ.");
                    }
                }
            }

            return pictureLinks;
        }
    }
}
