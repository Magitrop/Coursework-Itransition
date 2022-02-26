using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Coursework_Itransition.Data
{
    public class Review
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)] public string ReviewID { get; set; }
        public string ReviewCreatorID { get; set; }
        public string AttachedPictureLinks { get; set; }
        public string ReviewSubjectName { get; set; }
        public string ReviewSubjectGenre { get; set; }
        public string ReviewText { get; set; }
        [Range(0, 5)] public int OwnerRating { get; set; }

        public ICollection<UserReviewAndTagRelation> TagRelations { get; set; }
    }
}
