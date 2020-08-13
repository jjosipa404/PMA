using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PMANews.Areas.Identity.Data
{
    public class Post
    {
        //---------------------------------------------------- ID
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //---------------------------------------------------- NASLOV
        public string Title { get; set; }
        //---------------------------------------------------- SADRZAJ
        public string Content { get; set; }   
        //---------------------------------------------------- DATUM UREDJIVANJA
        [DataType(DataType.Date)]
        [Display(Name = "Updated")]
        public DateTime DateUpdated { get; set; }
        //---------------------------------------------------- KATEGORIJA
        [Display(Name = "Category")]
        public int CategoryId { get; set; }         
        public virtual Category Category { get; set; }
        //---------------------------------------------------- ODOBRENO
        [Display(Name = "Approved")]
        public int ApprovedId { get; set; }
        public virtual Approved Approved { get; set; }
        //---------------------------------------------------- AUTOR
        [Display(Name = "Author")]
        public string AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }
        //---------------------------------------------------- KOMENTARI
        public ICollection<Comment> Comments { get; set; }
    }
}
