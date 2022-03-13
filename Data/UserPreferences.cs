using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RazorCoursework.Data
{
    public class UserPreferences
    {
        [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)] public string PreferenceID { get; set; }
        public string UserID { get; set; }
        public bool IsDarkTheme { get; set; }
        public bool IsEnglishVersion { get; set; }
    }
}
