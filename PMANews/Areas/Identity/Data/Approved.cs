using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PMANews.Areas.Identity.Data
{
    public class Approved
    {
        //----------------------------------------------------
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //----------------------------------------------------
        public string Name { get; set; }
 
        //----------------------------------------------------
        public ICollection<Post> Posts { get; set; }
    }
}
