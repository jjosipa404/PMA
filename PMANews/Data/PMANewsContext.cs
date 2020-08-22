using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PMANews.Areas.Identity.Data;

namespace PMANews.Data
{
    public class PMANewsContext : IdentityDbContext
    {
        public DbSet<ApplicationUser> User { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Rank> Rank { get; set; }
        public DbSet<Approved> Approved { get; set; }

        public PMANewsContext(DbContextOptions<PMANewsContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            //-- one to many relationships ------------------
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.AuthorId);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Approved)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.ApprovedId);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Rank)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RankId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comment>()
              .Property(c => c.DateCreated)
              .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<Post>()
               .Property(p => p.DateUpdated)
               .HasDefaultValueSql("getdate()");


            //-----------------------------------------------
        }
    }
}
