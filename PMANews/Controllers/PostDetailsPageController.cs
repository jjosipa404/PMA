using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMANews.Areas.Identity.Data;
using PMANews.Data;

namespace PMANews.Controllers
{
   
    public class PostDetailsPageController : Controller
    {
        private readonly PMANewsContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public PostDetailsPageController(PMANewsContext context, 
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> PostDetailsPage(int id)
        {
            if (_signInManager.IsSignedIn(User))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = User.FindFirstValue(ClaimTypes.Name);
                ApplicationUser appUser = _context.User.Include(u => u.Rank).Where(u => u.Id == userId).FirstOrDefault();
                ViewBag.LoggedInUserRank = appUser.Rank.Name;
            }

            PostDetailsPageModels models = new PostDetailsPageModels();
            models.Post = await _context.Post
                .Include(p => p.Approved)
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();


            models.RelatedPosts = await _context.Post
                .Include(p => p.Author)
                .Include(p => p.Category)
                .Include(p => p.Approved)
                .Where(p => p.CategoryId == models.Post.CategoryId)
                .Where(p => p.Approved.Name == "Yes")
                .OrderByDescending(p => p.DateUpdated)
                .ToListAsync();
     
            if(models.Post != null & models.RelatedPosts != null)
            {
                for (int i = 0; i <= models.RelatedPosts.Count - 1; i++)
                { 
                    if(models.RelatedPosts[i].Id == models.Post.Id)
                    {
                        models.RelatedPosts.RemoveAt(i);
                        break;
                    }
                }                
            }


            if (models.RelatedPosts.Count > 5)
            {
                models.RelatedPosts.RemoveRange(5, models.RelatedPosts.Count - 5);

            }
            return View(models);

        }
    }
}
