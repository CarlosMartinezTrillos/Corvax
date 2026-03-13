using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Foro.Entities.Models;
using Foro_Militar.Models;
using System.Data.Entity;
using System.Web.Mvc;

namespace Foro_Militar.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly AppDbContext _context = new AppDbContext();

        // ── GET /communities/{slug}/create-post ──────────────────────
        [HttpGet]
        [Route("communities/{slug}/create-post")]
        public ActionResult Create(string slug)
        {
            var community = _context.Communities
                .Include(c => c.Categories)
                .FirstOrDefault(c => c.Slug == slug && c.IsActive);

            if (community == null)
                return HttpNotFound();

            var model = new CreatePostViewModel
            {
                CommunityId = community.Id,
                CommunitySlug = community.Slug,
                CommunityName = community.Name,

                // Solo las categorías de esta comunidad → sección "Principal"
                AvailableCategories = community.Categories
                    .Where(c => c.IsActive)
                    .Select(c => new CategoryOption
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ColorHex = c.ColorHex
                    }).ToList(),

                // Todas las categorías activas de la BD → sección "Adicionales"
                AllCategories = _context.Set<Category>()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryOption
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ColorHex = c.ColorHex
                    }).ToList()
            };

            if (Request.QueryString["modal"] == "1")
                ViewBag.UseModalLayout = true;

            return View(model);
        }

        // ── POST /communities/{slug}/create-post ─────────────────────
        [HttpPost]
        [Route("communities/{slug}/create-post")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string slug, CreatePostViewModel model)
        {
            var community = _context.Communities
                .Include(c => c.Categories)
                .FirstOrDefault(c => c.Slug == slug && c.IsActive);

            if (community == null)
                return HttpNotFound();

            model.CommunityId = community.Id;
            model.CommunitySlug = community.Slug;
            model.CommunityName = community.Name;

            model.AvailableCategories = community.Categories
                .Where(c => c.IsActive)
                .Select(c => new CategoryOption
                {
                    Id = c.Id,
                    Name = c.Name,
                    ColorHex = c.ColorHex
                }).ToList();

            model.AllCategories = _context.Set<Category>()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryOption
                {
                    Id = c.Id,
                    Name = c.Name,
                    ColorHex = c.ColorHex
                }).ToList();

            if (Request.QueryString["modal"] == "1")
                ViewBag.UseModalLayout = true;

            if (!ModelState.IsValid)
                return View(model);

            int userId = int.Parse(User.Identity.Name);

            var post = new Post
            {
                Title = model.Title.Trim(),
                Content = model.Content.Trim(),
                Image = string.IsNullOrWhiteSpace(model.Image) ? null : model.Image.Trim(),
                UserId = userId,
                CommunityId = community.Id,
                MainCategoryId = model.MainCategoryId,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                Slug = GenerateSlug(model.Title)
            };

            post.PostType = model.PostType ?? "debate";
            _context.Posts.Add(post);
            _context.SaveChanges();

            _context.Set<PostCategory>().Add(new PostCategory
            {
                PostId = post.Id,
                CategoryId = model.MainCategoryId
            });

            if (model.ExtraCategoryIds != null)
            {
                foreach (var catId in model.ExtraCategoryIds.Distinct()
                                          .Where(id => id != model.MainCategoryId))
                {
                    _context.Set<PostCategory>().Add(new PostCategory
                    {
                        PostId = post.Id,
                        CategoryId = catId
                    });
                }
            }

            _context.SaveChanges();

            if (Request.QueryString["modal"] == "1")
            {
                return Content(@"
                    <script>
                        if (window.top && window.top.CorvaxDashboard)
                            window.top.CorvaxDashboard.closeCreatePost();
                        else
                            window.top.location.reload();
                    </script>
                ", "text/html");
            }

            return RedirectToAction("Dashboard", "Communities", new { slug = slug });
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return "";
            return title.ToLower()
                        .Replace(" ", "-")
                        .Replace("á", "a").Replace("é", "e")
                        .Replace("í", "i").Replace("ó", "o")
                        .Replace("ú", "u").Replace("ñ", "n");
        }
    }
}