﻿using System;
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
    public class PostsController : Controller
    {
        private readonly PMANewsContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public PostsController(PMANewsContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }


        [AllowAnonymous]
        // GET: /Posts/Index
        public async Task<IActionResult> Index()
        {
            if (_signInManager.IsSignedIn(User))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = User.FindFirstValue(ClaimTypes.Name);
                ApplicationUser appUser = _context.User.Include(u => u.Rank).Where(u => u.Id == userId).FirstOrDefault();
                ViewBag.LoggedInUserRank = appUser.Rank.Name;
            }

            //vraca listu svih postova koji su odobreni za objavu i koji ce se ispisivati na naslovnoj stranici sortirani po datumu objave
            var posts = _context.Post.Include(p => p.Category).Include(p => p.Author).Where(p => p.Approved.Name == "Yes").OrderByDescending(p => p.DateUpdated);  
            return View(await posts.ToListAsync());
        }

        [AllowAnonymous]
        // GET: /Posts/Filter/5
        public async Task<IActionResult> Filter(int? id)
        {
            if (_signInManager.IsSignedIn(User))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = User.FindFirstValue(ClaimTypes.Name);
                ApplicationUser appUser = _context.User.Include(u => u.Rank).Where(u => u.Id == userId).FirstOrDefault();
                ViewBag.LoggedInUser = appUser;
                ViewBag.LoggedInUserRank = appUser.Rank.Name;
            }
            if (id == null)
            {
                return NotFound();
            }
            //vraca listu svih postova koji su odobreni za objavu i koji ce se ispisivati sortirani po datumu objave i po kategoriji
            var posts = _context.Post.Include(p => p.Category).Include(p => p.Author).Where(p => p.CategoryId == id).Where(p => p.Approved.Name == "Yes").OrderByDescending(p => p.DateUpdated);
            return View(await posts.ToListAsync());
        }
        
        // GET: /Posts/MyPosts
        public async Task<IActionResult> MyPosts()
        {
           var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           var userName = User.FindFirstValue(ClaimTypes.Name);
           ApplicationUser appUser = _context.User.Include(u => u.Rank).Where(u => u.Id == userId).FirstOrDefault();
           ViewBag.LoggedInUser = appUser;
           ViewBag.LoggedInUserRank = appUser.Rank.Name;

           //ispisuju se u tablici svi postovi ulogiranog autora sortirani po datumu ažuriranja 
           var posts = _context.Post.Include(p => p.Category).Include(p => p.Approved).Include(p => p.Author).Where(p => p.Author.UserName == userName).OrderByDescending(p => p.DateUpdated);
           return View(await posts.ToListAsync());

        }

        // GET: /Posts/AllPosts
        public async Task<IActionResult> AllPosts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            ApplicationUser appUser = _context.User.Include(u => u.Rank).Where(u => u.Id == userId).FirstOrDefault();
            ViewBag.LoggedInUser = appUser;
            ViewBag.LoggedInUserRank = appUser.Rank.Name;

            //ispisuju se u tablici svi postovi sortirani po datumu ažuriranja 
            var posts = _context.Post.Include(p => p.Category).Include(p => p.Approved).Include(p => p.Author).OrderBy(p => p.Approved.Name);
            return View(await posts.ToListAsync());

        }

  

        [AllowAnonymous]
        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (_signInManager.IsSignedIn(User))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = User.FindFirstValue(ClaimTypes.Name);
                ApplicationUser appUser = _context.User.Include(u => u.Rank).Where(u => u.Id == userId).FirstOrDefault();
                ViewBag.UserRank = appUser.Rank.Name;
            }

            if (id == null)
            {
                return NotFound();
            }
            //dohvaca se post prema id-u koji se posalje kao parametar
            var post = await _context.Post
                .Include(p => p.Category)
                .Include(p => p.Author)
                .Include(p => p.Approved)
                .FirstOrDefaultAsync(p => p.Id == id);
            //dohvacaju se svi komentari koji pripadaju tom postu da se na View-u ispise ukupan broj komentara
            var comm = _context.Comment
                .Include(c => c.Post)
                .Include(c => c.User)
                .Where(c => c.PostId == id);
            if (post == null)
            {
                return NotFound();
            }
            else
            {
                ViewBag.NumberOfComments = comm.Count();
            }
            return View(post);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            if (IsInRank("admin") | IsInRank("author"))
            {
                ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name");
                return View();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,CategoryId")] Post post)
        {
            if(IsInRank("admin") | IsInRank("author"))   
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
                var userName = User.FindFirstValue(ClaimTypes.Name); 
                ApplicationUser appUser = await _userManager.GetUserAsync(User);

                post.Author = appUser;
                post.AuthorId = userId;

                post.ApprovedId = 2;  //u bazi u tablici Approved je pod id-om 1 "Yes", a pod 2 je "No", Approved "Yes" postavlja editor ili admin kao odobrenje za objavu
                post.Approved = _context.Approved.FirstOrDefault(a => a.Id == 2);

                if (ModelState.IsValid)
                {
                    _context.Add(post);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("MyPosts","Posts");
                }
                ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name", post.Category.Name);
                return View(post);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            ApplicationUser appUser = _context.User.Include(u => u.Rank).Where(u => u.Id == userId).FirstOrDefault();
            ViewBag.LoggedInUser = appUser;

            if (IsInRank("admin") | IsInRank("editor") | IsInRank("author"))
            {
                if (id == null)
                {
                    return NotFound();
                }
                var post = await _context.Post.FindAsync(id);
                if (post == null)
                {
                    return NotFound();
                }
                ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name");
                ViewData["AuthorId"] = new SelectList(_context.Set<ApplicationUser>().Where(u => u.Rank.Name == "author"), "Id", "UserName");
                ViewData["ApprovedId"] = new SelectList(_context.Set<Approved>(), "Id", "Name");
                return View(post);
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,AuthorId,CategoryId,ApprovedId")] Post post)
        {
            if (IsInRank("admin") | IsInRank("editor") | IsInRank("author"))
            {
                if (id != post.Id)
                {
                    return NotFound();
                }
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = User.FindFirstValue(ClaimTypes.Name); 
                ApplicationUser appUser = await _userManager.GetUserAsync(User);
                post.DateUpdated = DateTime.Now;
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(post);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!PostExists(post.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    if (IsInRank("author"))
                    {
                        return RedirectToAction("MyPosts", "Posts");
                    }
                    else if (IsInRank("editor"))
                    {
                        return RedirectToAction("AllPosts", "Posts");
                    }
                }
                ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "Id", "Name", post.Category.Name);
                ViewData["AuthorId"] = new SelectList(_context.Set<ApplicationUser>().Where(u => u.Rank.Name == "author"), "Id", "UserName", post.Author.UserName);
                ViewData["ApprovedId"] = new SelectList(_context.Set<Approved>(), "Id", "Name", post.Approved.Name);
                return View(post);
            }
            return RedirectToAction(nameof(Index));

        }

       
        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = await _context.Post
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Post.FindAsync(id);
            _context.Post.Remove(post);
            await _context.SaveChangesAsync();
            if (IsInRank("author"))
            {
                return RedirectToAction("MyPosts", "Posts");
            }
            else if (IsInRank("admin") | IsInRank("editor"))
            {
                return RedirectToAction("AllPosts", "Posts");
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Post.Any(e => e.Id == id);
        }
        
        public Boolean IsInRank(string rank) //"admin", "editor", "author" ili "user"
        {
            //metoda koja provjerava koju ulogu ima trenutno ulogirani korisnik:
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser appUser = _context.User.Include(u => u.Rank).Where(u => u.Id == userId).FirstOrDefault();

            if (appUser.Rank.Name == rank)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
