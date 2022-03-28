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
            using var context = AppContentDbContext.Create();
            Review = context.Reviews
                .Include(r => r.TagRelations)
                .ThenInclude(r => r.Tag)
                .FirstOrDefault(r => r.ReviewID == id);
            if (Review == null)
                return NotFound();

            ReviewRating = GetRating(id);
            ReviewAuthorRating = GetAuthorRating(id);
            return Page();
        }

        public bool CheckUserOwnership()
        {
            using var context = AppContentDbContext.Create();
            string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return User.Identity.IsAuthenticated && (Review.ReviewCreatorID == currentUserID || User.IsInRole("Admin"));
        }

        public async Task<IActionResult> OnPostLike()
        {
            using StreamReader reader = new StreamReader(await CreateMemoryStreamFromRequest());
            using var context = AppContentDbContext.Create();

            string reviewID = string.Empty;
            bool alreadyLiked = false;

            reviewID = await reader.ReadToEndAsync();
            var currentReview = await context.Reviews.FirstOrDefaultAsync(r => r.ReviewID == reviewID);
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
            return new JsonResult(GetLikesCount(reviewID).ToString() + ';' + (!alreadyLiked).ToString());
        }

        public async Task<IActionResult> OnPostRating()
        {
            using StreamReader reader = new StreamReader(await CreateMemoryStreamFromRequest());
            using var context = AppContentDbContext.Create();

            bool shouldRemoveRating = false;
            var data = (await reader.ReadToEndAsync()).Split(';');
            string reviewID = data[0];
            int.TryParse(data[1], out int ratingValue);

            var currentReview = await context.Reviews.FirstOrDefaultAsync(r => r.ReviewID == reviewID);
            if (currentReview != null)
            {
                string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                shouldRemoveRating = await ChangeRating(context, currentReview, currentUserID, ratingValue);
            }
            return new JsonResult(shouldRemoveRating.ToString() + ";" + GetAverageRating(reviewID).ToString());
        }

        private async Task<MemoryStream> CreateMemoryStreamFromRequest()
        {
            MemoryStream stream = new MemoryStream();
            await Request.Body.CopyToAsync(stream);
            stream.Position = 0;
            return stream;
        }

        private async Task<bool> ChangeRating(AppContentDbContext context, Review currentReview, string currentUserID, int ratingValue)
        {
            bool shouldRemoveRating = false;
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
            return shouldRemoveRating;
        }

        public bool AlreadyLikedReview(string reviewID)
        {
            using var context = AppContentDbContext.Create();
            string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return context.ReviewLikes.Any(like =>
                like.UserID == currentUserID &&
                like.ReviewID == reviewID);
        }

        public int GetLikesCount(string reviewID)
        {
            using var context = AppContentDbContext.Create();
            return context.ReviewLikes.Where(like => like.ReviewID == reviewID).Count();
        }

        public int GetRating(string reviewID)
        {
            using var context = AppContentDbContext.Create();
            string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return context.ReviewRatings.Where(rating =>
                    rating.ReviewID == reviewID &&
                    rating.UserID == currentUserID).FirstOrDefault()?.RatingValue ?? 0;
        }

        public int GetAuthorRating(string reviewID)
        {
            using var context = AppContentDbContext.Create();
            string authorID = Review.ReviewCreatorID;
            return context.ReviewRatings.Where(rating =>
                    rating.ReviewID == reviewID &&
                    rating.UserID == authorID).FirstOrDefault()?.RatingValue ?? 0;
        }

        public double GetAverageRating(string reviewID)
        {
            using var context = AppContentDbContext.Create();
            string currentUserID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var ratings = context.ReviewRatings.Where(rating => rating.ReviewID == reviewID);
            double count = ratings.Count();
            if (count == 0) 
                return 0;
            else 
                return Math.Round(ratings.Sum(rating => rating.RatingValue) / count, 3);
        }

        public int GetCreatorLikesCount(string userID)
        {
            using var context = AppContentDbContext.Create();
            var creatorReviews = context.Reviews.Where(r => r.ReviewCreatorID == userID);
            return context.ReviewLikes.Where(like => creatorReviews.Any(r => r.ReviewID == like.ReviewID)).Count();
        }
    }
}
