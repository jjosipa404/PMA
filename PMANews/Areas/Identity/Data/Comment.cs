using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace PMANews.Areas.Identity.Data
{
    public class Comment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }
        public string CommContent { get; set; }

        [Display(Name = "Post")]
        [NotNull]
        public int PostId { get; set; }
        public virtual Post Post { get; set; }

        [Display(Name = "User")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
