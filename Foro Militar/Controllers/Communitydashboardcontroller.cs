using Foro.API.DTOs.Community;
using Foro.Entities.Models;

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Foro.API.Controllers
{
    [RoutePrefix("api/communities")]
    public class CommunityDashboardController : Controller
    {
        private readonly AppDbContext _context;

        // cache global de la app
        private static readonly ObjectCache _cache = MemoryCache.Default;

        public CommunityDashboardController()
        {
            _context = new AppDbContext();
        }

        // ══════════════════════════════════════════════════════════════
        // GET /api/communities/{slug}/dashboard
        // ══════════════════════════════════════════════════════════════

        [HttpGet]
        [Route("{slug}/dashboard")]
        public async Task<ActionResult> GetDashboard(string slug)
        {
            var currentUserId = GetCurrentUserId();
            var cacheKey = "dashboard:" + slug;

            CommunityDashboardDto dto = _cache.Get(cacheKey) as CommunityDashboardDto;

            if (dto == null)
            {
                var community = await _context.Communities
                    .Include(c => c.CreatedByUser)
                    .Include(c => c.Rank)
                    .Include(c => c.UserCommunities.Select(uc => uc.User))
                    .Include(c => c.Categories)
                    .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

                if (community == null)
                    return HttpNotFound("Comunidad no encontrada.");

                // ── Validar visibilidad ──

                if (community.Visibility == 2 && currentUserId == null)
                {
                    Response.StatusCode = 403;

                    return Json(new
                    {
                        message = "Esta comunidad es privada.",
                        name = community.Name,
                        description = community.Description
                    }, JsonRequestBehavior.AllowGet);
                }

                if (community.Visibility == 2 && currentUserId != null)
                {
                    var isMember = await _context.Set<UserCommunity>()
                        .AnyAsync(x =>
                            x.CommunityId == community.Id &&
                            x.UserId == currentUserId.Value);

                    if (!isMember)
                    {
                        Response.StatusCode = 403;

                        return Json(new
                        {
                            message = "Esta comunidad es privada.",
                            name = community.Name
                        }, JsonRequestBehavior.AllowGet);
                    }
                }

                // ── Conteos base ──

                var totalPosts = await _context.Posts
                    .CountAsync(p => p.CommunityId == community.Id && !p.IsDeleted);

                var totalComments = await _context.Comments
                    .CountAsync(c => c.Post.CommunityId == community.Id && !c.IsDeleted);

                var totalFollowers = community.UserCommunities.Count;

                // ── Top miembros ──

                var topMembers = await _context.Posts
                    .Where(p => p.CommunityId == community.Id && !p.IsDeleted)
                    .GroupBy(p => new { p.UserId, p.User.Username })
                    .Select(g => new TopMemberDto
                    {
                        UserId = g.Key.UserId,
                        Username = g.Key.Username,
                        PostCount = g.Count(),
                        CommentCount = _context.Comments.Count(c =>
                            c.UserId == g.Key.UserId &&
                            c.Post.CommunityId == community.Id &&
                            !c.IsDeleted)
                    })
                    .OrderByDescending(m => m.PostCount * 2 + m.CommentCount)
                    .Take(5)
                    .ToListAsync();

                for (int i = 0; i < topMembers.Count; i++)
                    topMembers[i].Rank = i + 1;

                // ── Stats por categoría ──

                var categoryStats = await _context.Posts
                    .Where(p => p.CommunityId == community.Id && !p.IsDeleted)
                    .SelectMany(p => p.PostCategories)
                    .GroupBy(pc => new
                    {
                        pc.CategoryId,
                        pc.Category.Name,
                        pc.Category.ColorHex
                    })
                    .Select(g => new CategoryStatsDto
                    {
                        Id = g.Key.CategoryId,
                        Name = g.Key.Name,
                        ColorHex = g.Key.ColorHex,
                        PostCount = g.Count()
                    })
                    .OrderByDescending(x => x.PostCount)
                    .Take(6)
                    .ToListAsync();

                if (totalPosts > 0)
                {
                    foreach (var c in categoryStats)
                    {
                        c.Percentage = Math.Round((double)c.PostCount / totalPosts * 100, 1);
                    }
                }

                // ── Moderadores ──

                var moderators = community.UserCommunities
                    .Where(uc => uc.UserId == community.CreatedByUserId)
                    .Select(uc => new ModeratorDto
                    {
                        UserId = uc.UserId,
                        Username = uc.User.Username,
                        IsOwner = uc.UserId == community.CreatedByUserId
                    })
                    .OrderByDescending(m => m.IsOwner)
                    .ToList();

                // ── Actividad reciente ──

                // ✅ DESPUÉS
                var recentPostsRaw = await _context.Posts
                    .Where(p => p.CommunityId == community.Id && !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .Select(p => new
                    {
                        p.User.Username,
                        p.Title,
                        p.CreatedAt
                    })
                    .ToListAsync(); // ← SQL termina aquí

                // Ahora Math.Min corre en C#, no en SQL
                var recentPosts = recentPostsRaw.Select(p => new RecentActivityDto
                {
                    Type = "post",
                    Username = p.Username,
                    Summary = "publicó \"" + p.Title.Substring(0, Math.Min(p.Title.Length, 40)) + "\"",
                    CreatedAt = p.CreatedAt
                }).ToList();

                // ✅ DESPUÉS
                var recentCommentsRaw = await _context.Comments
                    .Where(c => c.Post.CommunityId == community.Id && !c.IsDeleted)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .Select(c => new
                    {
                        c.User.Username,
                        PostTitle = c.Post.Title,
                        c.CreatedAt
                    })
                    .ToListAsync(); // ← SQL termina aquí

                var recentComments = recentCommentsRaw.Select(c => new RecentActivityDto
                {
                    Type = "comment",
                    Username = c.Username,
                    Summary = "comentó en \"" + c.PostTitle.Substring(0, Math.Min(c.PostTitle.Length, 35)) + "\"",
                    CreatedAt = c.CreatedAt
                }).ToList();

                var recentJoins = community.UserCommunities
                    .OrderByDescending(uc => uc.JoinedAt)
                    .Take(5)
                    .Select(uc => new RecentActivityDto
                    {
                        Type = "join",
                        Username = uc.User.Username,
                        Summary = "se unió a la comunidad",
                        CreatedAt = uc.JoinedAt
                    })
                    .ToList();

                var recentActivity = recentPosts
                    .Concat(recentComments)
                    .Concat(recentJoins)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .ToList();

                // ── Gráfico semanal ──

                var since = DateTime.Now.AddDays(-6).Date;

                var weeklyRaw = await _context.Posts
                    .Where(p =>
                        p.CommunityId == community.Id &&
                        p.CreatedAt >= since &&
                        !p.IsDeleted)
                    .GroupBy(p => DbFunctions.TruncateTime(p.CreatedAt))
                    .Select(g => new DailyPostCountDto
                    {
                        Date = g.Key.Value,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var weeklyChart = Enumerable.Range(0, 7)
                    .Select(i => since.AddDays(i))
                    .Select(date =>
                        weeklyRaw.FirstOrDefault(w => w.Date == date)
                        ?? new DailyPostCountDto
                        {
                            Date = date,
                            Count = 0
                        })
                    .ToList();

                // ── DTO final ──

                dto = new CommunityDashboardDto
                {
                    Id = community.Id,
                    Name = community.Name,
                    Slug = community.Slug,
                    Description = community.Description,
                    ImageUrl = community.ImageUrl,
                    BannerUrl = community.BannerUrl,
                    Rules = community.Rules,
                    Visibility = community.Visibility,
                    CreatedAt = community.CreatedAt,

                    TotalFollowers = totalFollowers,
                    TotalPosts = totalPosts,
                    TotalComments = totalComments,

                    UpVotes = community.UpVotes,
                    DownVotes = community.DownVotes,
                    PowerScore = community.PowerScore,

                    WeeklyNewFollowers = community.WeeklyNewFollowers,
                    WeeklyPosts = community.WeeklyPosts,
                    WeeklyComments = community.WeeklyComments,

                    RankName = community.Rank != null ? community.Rank.Name : null,
                    RankPosition = community.RankId,

                    TopMembers = topMembers,
                    CategoryStats = categoryStats,
                    Moderators = moderators,
                    RecentActivity = recentActivity,
                    WeeklyPostChart = weeklyChart
                };

                _cache.Set(cacheKey, dto, DateTimeOffset.Now.AddMinutes(5));
            }

            // ── Campos personalizados ──

            if (currentUserId.HasValue)
            {
                var relation = await _context.Set<UserCommunity>()
                    .FirstOrDefaultAsync(x =>
                        x.CommunityId == dto.Id &&
                        x.UserId == currentUserId.Value);

                dto.IsFollowing = relation != null;

                dto.IsOwner = await _context.Communities
                    .AnyAsync(c =>
                        c.Id == dto.Id &&
                        c.CreatedByUserId == currentUserId.Value);
            }

            return Json(dto, JsonRequestBehavior.AllowGet);
        }

        // ══════════════════════════════════════════════════════════════
        // FOLLOW / UNFOLLOW
        // ══════════════════════════════════════════════════════════════

        [HttpPost]
        [Route("{id}/follow")]
        public async Task<ActionResult> ToggleFollow(int id)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                Response.StatusCode = 401;
                return Json(new { message = "Debes iniciar sesión." });
            }

            var relation = await _context.Set<UserCommunity>()
                .FirstOrDefaultAsync(x =>
                    x.CommunityId == id &&
                    x.UserId == userId.Value);

            if (relation == null)
            {
                relation = new UserCommunity
                {
                    CommunityId = id,
                    UserId = userId.Value,
                    JoinedAt = DateTime.Now
                };

                _context.Set<UserCommunity>().Add(relation);
            }
            else
            {
                _context.Set<UserCommunity>().Remove(relation);
            }

            await _context.SaveChangesAsync();

            // limpiar cache dashboard
            var slug = await _context.Communities
                .Where(c => c.Id == id)
                .Select(c => c.Slug)
                .FirstOrDefaultAsync();

            if (slug != null)
            {
                _cache.Remove("dashboard:" + slug);
            }

            return Json(new
            {
                success = true,
                isFollowing = relation != null
            });
        }

        // ══════════════════════════════════════════════════════════════
        // OBTENER USER ID
        // ══════════════════════════════════════════════════════════════

        private int? GetCurrentUserId()
        {
            var identity = User.Identity as ClaimsIdentity;

            if (identity == null)
                return null;

            var claim = identity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                return null;

            int id;

            if (int.TryParse(claim.Value, out id))
                return id;

            return null;
        }

        [HttpGet]
        [Route("{slug}/posts")]
        public async Task<ActionResult> GetPosts(string slug, string sort = "hot", int page = 1)
        {
            int pageSize = 20;

            var community = await _context.Communities
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

            if (community == null)
                return HttpNotFound();

            IQueryable<Post> query = _context.Posts
                .Include(p => p.User)
                .Where(p => p.CommunityId == community.Id && !p.IsDeleted);

            switch (sort)
            {
                case "new":
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
                case "top":
                    query = query.OrderByDescending(p =>
                        p.Votes.Count(v => v.VoteType == 1) - p.Votes.Count(v => v.VoteType == -1));
                    break;
                case "comments":
                    query = query.OrderByDescending(p => p.Comments.Count);
                    break;
                default: // hot
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            var totalCount = await query.CountAsync();

            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    ContentExcerpt = p.Content != null
                        ? p.Content.Substring(0, p.Content.Length > 150 ? 150 : p.Content.Length)
                        : "",
                    Username = p.User.Username,
                    p.CreatedAt,
                    UpVotes = p.Votes.Count(v => v.VoteType == 1),
                    DownVotes = p.Votes.Count(v => v.VoteType == -1),
                    Score = p.Votes.Count(v => v.VoteType == 1) - p.Votes.Count(v => v.VoteType == -1),
                    CommentCount = p.Comments.Count(c => !c.IsDeleted)
                })
                .ToListAsync();

            return Json(new
            {
                Items = posts,
                TotalCount = totalCount,
                PageSize = pageSize,
                Page = page
            }, JsonRequestBehavior.AllowGet);
        }

    }
}