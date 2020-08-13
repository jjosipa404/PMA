using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PMANews.Areas.Identity.Data
{
    public class AplicationUserPost
    {
        [Key, Column(Order = 1)]
        public int PostId { get; set; }

        [Key, Column(Order = 2)]
        public int ApplicationUserId { get; set; }

        public virtual Post Post { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
