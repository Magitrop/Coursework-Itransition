using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
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
        public int ReviewRating { get; set; }
        public int ReviewAuthorRating { get; set; }

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
            }
            ReviewRating = GetRating(id);
            ReviewAuthorRating = GetAuthorRating(id);
            return Page();
        }

        public async Task<IActionResult> OnPostLike()
        {
            MemoryStream stream = new MemoryStream();
            await Request.Body.CopyToAsync(stream);
            stream.Position = 0;
            string reviewID = string.Empty;
            bool alreadyLiked = false;
            using (StreamReader reader = new StreamReader(stream))
            {
                reviewID = await reader.ReadToEndAsync();
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
                    var currentReview = await reviews.FirstOrDefaultAsync(r => r.ReviewID == reviewID);
                    if (currentReview != null)
                    {
                        string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                        Like like = await context.ReviewLikes
                            .FirstOrDefaultAsync(l =>
                            l.ReviewID == currentReview.ReviewID &&
                            l.UserID == currentUserID);
                        if (alreadyLiked = like == null)
                        {
                            like = new Like()
                            {
                                Review = currentReview,
                                UserID = currentUserID
                            };
                            await context.ReviewLikes.AddAsync(like);
                        }
                        else
                            context.ReviewLikes.Remove(like);
                        await context.SaveChangesAsync();
                    }
                }
            }
            return new JsonResult(GetLikesCount(reviewID).ToString() + ';' + (!alreadyLiked).ToString());
        }

        public async Task<IActionResult> OnPostRating()
        {
            MemoryStream stream = new MemoryStream();
            await Request.Body.CopyToAsync(stream);
            stream.Position = 0;
            string reviewID = string.Empty;
            bool shouldRemoveRating = false;
            using (StreamReader reader = new StreamReader(stream))
            {
                var data = (await reader.ReadToEndAsync()).Split(';');
                reviewID = data[0];
                int ratingValue = int.Parse(data[1]);
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
                    var currentReview = await reviews.FirstOrDefaultAsync(r => r.ReviewID == reviewID);
                    if (currentReview != null)
                    {
                        string currentUserID = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                        Rating rating = await context.ReviewRatings
                            .FirstOrDefaultAsync(l =>
                                l.ReviewID == currentReview.ReviewID &&
                                l.UserID == currentUserID);
                        if (rating == null)
                        {
                            rating = new Rating()
                            {
                                Review = currentReview,
                                UserID = currentUserID,
                                RatingValue = ratingValue
                            };
                            await context.ReviewRatings.AddAsync(rating);
                        }
                        else if (shouldRemoveRating = rating.RatingValue == ratingValue)
                            context.ReviewRatings.Remove(rating);
                        else
                            rating.RatingValue = ratingValue;
                        await context.SaveChangesAsync();
                    }
                }
            }
            return new JsonResult(shouldRemoveRating.ToString() + ";" + GetAverageRating(reviewID).ToString());
        }

        public bool AlreadyLikedReview(string reviewID)
        {
            bool result = true;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                string currentUserID = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                result = context.ReviewLikes.Any(like =>
                    like.UserID == currentUserID &&
                    like.ReviewID == reviewID);
            }
            return result;
        }

        public int GetLikesCount(string reviewID)
        {
            int result = 0;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                result = context.ReviewLikes.Where(like => like.ReviewID == reviewID).Count();
            }
            return result;
        }

        public int GetRating(string reviewID)
        {
            int result;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                result = context.ReviewRatings.Where(rating =>
                    rating.ReviewID == reviewID &&
                    rating.UserID == currentUserID).FirstOrDefault()?.RatingValue ?? 0;
            }
            return result;
        }

        public int GetAuthorRating(string reviewID)
        {
            int result;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                string authorID = Review.ReviewCreatorID;
                result = context.ReviewRatings.Where(rating =>
                    rating.ReviewID == reviewID &&
                    rating.UserID == authorID).FirstOrDefault()?.RatingValue ?? 0;
            }
            return result;
        }

        public double GetAverageRating(string reviewID)
        {
            double result;
            using (var context = new AppContentDbContext(
                   new DbContextOptionsBuilder<AppContentDbContext>()
                   .UseSqlServer(Startup.Connection)
                   .Options))
            {
                string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var ratings = context.ReviewRatings
                    .Where(rating => rating.ReviewID == reviewID);
                double count = ratings.Count();
                if (count == 0)
                    result = 0;
                else result = Math.Round(ratings.Sum(rating => rating.RatingValue) / count, 3);
            }
            return result;
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
    }
}
