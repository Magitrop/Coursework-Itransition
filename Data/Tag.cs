using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RazorCoursework.Data
{
    public class Tag
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)] public string TagID { get; set; }
        public string TagName { get; set; }

        public ICollection<UserReviewAndTagRelation> ReviewRelations { get; set; }
    }
}
