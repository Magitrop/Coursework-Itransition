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
using Microsoft.AspNetCore.Http;
using CG.Web.MegaApiClient;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Dropbox.Api;

namespace RazorCoursework.Pages
{
    [Authorize]
    public class CreateUserReviewModel : PageModel
    {
        private readonly IWebHostEnvironment _appEnvironment;
        public CreateUserReviewModel(IWebHostEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }

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
            [DataType(DataType.MultilineText)]
            //[MinLength(length: 100, ErrorMessage = "Текст обзора должен быть не короче 100 символов.")]
            public string ReviewText { get; set; }

            [Display(Name = "Теги (указываются через запятую)")]
            public string Tags { get; set; }
        }

        public async Task<IActionResult> OnPostTags()
        {
            string[] result = new string[0];
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                result = await (from t in context.Tags
                         where t.TagName.StartsWith(Request.Form["term"])
                         select t.TagName)
                         .Take(10)
                         .ToArrayAsync();
            }
            return new JsonResult(new { suggestions = result });
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
                    var newReview = new Review()
                    {
                        ReviewCreatorID = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value,
                        ReviewCreatorName = User.Identity.Name,
                        ReviewText = Input.ReviewText,
                        ReviewSubjectGenre = Input.ReviewSubjectGenre,
                        ReviewSubjectName = Input.ReviewSubjectName,
                        CreationDate = DateTime.Now,
                        TagRelations = new List<UserReviewAndTagRelation>(),
                        AttachedPictureLinks = pictureLinks
                    };
                    context.Reviews.Add(newReview);

                    if (Input.Tags?.Length > 0)
                    {
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
                    }

                    context.SaveChanges();
                }
            }

            return RedirectToPage("/Home", new { user = User.Identity.Name, p = 1 });
        }

        private async Task<string> GetPictureLinks()
        {
            if (Request.Form.Files == null || Request.Form.Files.Count == 0)
                return string.Empty;

            var tempDirectory = _appEnvironment.WebRootPath + "/files/";
            if (!Directory.Exists(tempDirectory))
                Directory.CreateDirectory(tempDirectory);

            string pictureLinks = string.Empty;
            using (var dbx = new DropboxClient("sl.BD-rvojmRetHizU_K9JtztYfcE-WInTNgZmaq3n1JNPU6Vo0_Erg4YmxX4bqdOPSHa2Q91ErOm0fb_RaDI1LOPAT7AiPOzf_fpgYMs2HrHJ-5gQdfNEqFpIELnRketeaeeloXKwOQOcP"))
            {
                foreach (var file in Request.Form.Files)
                {
                    string filepath = string.Empty;
                    if (file.Length > 0)
                    {
                        using (var stream = new FileStream(
                            tempDirectory + Guid.NewGuid() + "_" + file.FileName, FileMode.CreateNew))
                        {
                            file.CopyTo(stream);
                            filepath = stream.Name;
                        }
                    }
                    using (var fileStream = System.IO.File.Open(filepath, FileMode.Open))
                    {
                        //if (fileStream.Length <= 4096 * 1024)
                        {
                            var uploaded = await dbx.Files.UploadAsync(
                                "/" + Guid.NewGuid() + "_" + file.FileName,
                                body: fileStream);
                            pictureLinks += 
                                (await dbx.Sharing.CreateSharedLinkWithSettingsAsync(uploaded.PathLower))
                                .Url.Replace("dl=0", "raw=1") + ";";
                            //pictureLinks += (await dbx.Files.GetTemporaryLinkAsync(uploaded.PathLower)).Link + ";";
                        }
                    }
                    System.IO.File.Delete(filepath);
                }
            }

            return pictureLinks;
        }
    }
}

[AttributeUsage(
    AttributeTargets.Property |
    AttributeTargets.Field | 
    AttributeTargets.Parameter, 
    AllowMultiple = false)]
public sealed class MaxFilesCountAttribute : ValidationAttribute
{
    private readonly int _maxFilesCount;
    public MaxFilesCountAttribute(int maxFilesCount)
    {
        _maxFilesCount = maxFilesCount;
    }

    public override bool IsValid(object value)
    {
        var file = value as IFormFileCollection;
        return file != null && file.Count <= _maxFilesCount;
    }
}