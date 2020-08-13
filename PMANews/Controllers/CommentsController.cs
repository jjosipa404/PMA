using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PMANews.Areas.Identity.Data;
using PMANews.Data;

namespace PMANews.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly PMANewsContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(PMANewsContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        // GET: Comments
        public async Task<IActionResult> Index(int id)
        {
            ViewBag.PostID = id;
            var pMANewsContext = _context.Comment.Include(c => c.Post).Include(c => c.User).Where(c => c.PostId == id).OrderByDescending(c => c.DateCreated);
            return View(await pMANewsContext.ToListAsync());
        }

        [AllowAnonymous]
        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var comment = await _context.Comment
                .Include(c => c.Post)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }
            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create(int id)
        {
            return View();
        }

        // POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CommContent")] Comment comment, int postid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId
            ApplicationUser appUser = await _userManager.GetUserAsync(User);
            comment.User = appUser;
            comment.UserId = userId;
            comment.PostId = postid;
            comment.Post = _context.Post.Include(p => p.Author).Include(p => p.Category).Where(p => p.Id == postid).FirstOrDefault();

            if (ModelState.IsValid)
            {
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", postid);
            }
      
            return View(comment);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["PostId"] = new SelectList(_context.Set<Post>(), "Id", "Id", comment.PostId);
            ViewData["UserId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", comment.UserId);
            return View(comment);
        }


        // POST: Comments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CommContent,PostId,UserId")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    comment.DateCreated = DateTime.Now;
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("PostDetailsPage", "PostDetailsPage", new { id = comment.PostId });
            }
            ViewData["PostId"] = new SelectList(_context.Set<Post>(), "Id", "Id", comment.PostId);
            ViewData["UserId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", comment.UserId);
            return View(comment);
        }

        [Authorize]
        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment
                .Include(c => c.Post)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }
            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comment.FindAsync(id);
            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("PostDetailsPage", "PostDetailsPage", new { id = comment.PostId });
        }

        private bool CommentExists(int id)
        {
            return _context.Comment.Any(e => e.Id == id);
        }

        /**************************************************************************************/
        [HttpPost]
        public ActionResult AddComment(Comment comment, int postId)
        { 
            Comment comm = null;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.User.Include(u => u.Rank).FirstOrDefault(u => u.Id == userId);
            var post = _context.Post.Include(p => p.Author).Include(p => p.Category).Include(p => p.Approved).FirstOrDefault(p => p.Id == postId);

            if (comment != null)
            {
                comm = new Comment
                {
                    CommContent = comment.CommContent,
                    DateCreated = comment.DateCreated,
                    User = user,
                    UserId = userId,
                    Post = post,
                    PostId = postId

                };
                if (user != null && post != null)
                {
                    _context.Add(comm);
                    post.Comments.Add(comm);
                    user.Comments.Add(comm);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("GetComments", "Comments", new { postId = postId });
        }

        [AllowAnonymous]
        public PartialViewResult GetComments(int postId)
        {
            var comm = _context.Comment
               .Include(c => c.Post)
               .Include(c => c.User)
               .Where(c => c.PostId == postId);
            ViewBag.NumberOfComments = comm.Count();

            IQueryable<Comment> comments = _context.Comment.Where(c => c.Post.Id == postId)
                                     .Select(c => new Comment
                                     {
                                         Id = c.Id,
                                         DateCreated = c.DateCreated,
                                         CommContent = c.CommContent,
                                         User = c.User,
                                         UserId = c.UserId
                                     }).OrderByDescending(p => p.DateCreated).AsQueryable();

            return PartialView("~/Views/Shared/_MyComments.cshtml", comments);
        }
        
        /**************************************************************************************/
    }
}
