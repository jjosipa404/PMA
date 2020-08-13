using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMANews.Areas.Identity.Data
{
    public class PostDetailsPageModels
    {
        public Post Post { get; set; }
        public List<Post> RelatedPosts { get; set; }
    }
}
