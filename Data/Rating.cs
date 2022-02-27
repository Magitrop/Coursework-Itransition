using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RazorCoursework.Data
{
    public class Rating
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)] public string RatingID { get; set; }
        public string ReviewID { get; set; }
        public Review Review { get; set; }
        public string UserID { get; set; }
        [Range(1, 5)] public int RatingValue { get; set; }
    }
}
