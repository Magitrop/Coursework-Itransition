using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RazorCoursework.Data
{
    public class UserReviewAndTagRelation
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)] public string RelationID { get; set; }
        public string ReviewID { get; set; }
        public virtual Review Review { get; set; }
        public string TagID { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
