using Foro.API.DTOs.Community;
using Foro.Entities.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Foro_Militar.Services
{
    public class CommunityDashboardService
    {
        private readonly AppDbContext _context;

        public CommunityDashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CommunityDashboardDto> BuildAsync(string slug, int? currentUserId = null)
        {
            var community = await _context.Communities
                .Include(c => c.CreatedByUser)
                .Include(c => c.Rank)
                .Include(c => c.UserCommunities.Select(uc => uc.User))
                .Include(c => c.Categories)
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

            if (community == null) return null;

            var rankService = new CommunityRankService(_context);
            rankService.RecalculateRank(community);
            await _context.SaveChangesAsync();

            // Recargar Rank si cambió
            if (community.RankId.HasValue && community.Rank == null)
            {
                community.Rank = await _context.CommunityRanks
                    .FirstOrDefaultAsync(r => r.Id == community.RankId.Value);
            }


            var totalPosts = await _context.Posts
                .CountAsync(p => p.CommunityId == community.Id && !p.IsDeleted);

            var totalComments = await _context.Comments
                .CountAsync(c => c.Post.CommunityId == community.Id && !c.IsDeleted);

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

            // ── Category stats ──
            var categoryStats = await _context.Posts
                .Where(p => p.CommunityId == community.Id && !p.IsDeleted)
                .SelectMany(p => p.PostCategories)
                .GroupBy(pc => new { pc.CategoryId, pc.Category.Name, pc.Category.ColorHex })
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
                foreach (var c in categoryStats)
                    c.Percentage = Math.Round((double)c.PostCount / totalPosts * 100, 1);

            // ── Moderadores ──
            var moderators = community.UserCommunities
                .Where(uc => uc.UserId == community.CreatedByUserId)
                .Select(uc => new ModeratorDto
                {
                    UserId = uc.UserId,
                    Username = uc.User.Username,
                    IsOwner = uc.UserId == community.CreatedByUserId
                })
                .ToList();

            // ── Actividad reciente ──
            var recentPostsRaw = await _context.Posts
                .Where(p => p.CommunityId == community.Id && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new { p.User.Username, p.Title, p.CreatedAt })
                .ToListAsync();

            var recentPosts = recentPostsRaw.Select(p => new RecentActivityDto
            {
                Type = "post",
                Username = p.Username,
                Summary = "publicó \"" + p.Title.Substring(0, Math.Min(p.Title.Length, 40)) + "\"",
                CreatedAt = p.CreatedAt
            }).ToList();

            var recentCommentsRaw = await _context.Comments
                .Where(c => c.Post.CommunityId == community.Id && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new { c.User.Username, PostTitle = c.Post.Title, c.CreatedAt })
                .ToListAsync();

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
                }).ToList();

            var recentActivity = recentPosts
                .Concat(recentComments)
                .Concat(recentJoins)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToList();

            // ── Gráfico semanal ──
            var since = DateTime.Now.AddDays(-6).Date;

            var weeklyRaw = await _context.Posts
                .Where(p => p.CommunityId == community.Id && p.CreatedAt >= since && !p.IsDeleted)
                .GroupBy(p => DbFunctions.TruncateTime(p.CreatedAt))
                .Select(g => new DailyPostCountDto { Date = g.Key.Value, Count = g.Count() })
                .ToListAsync();

            var weeklyChart = Enumerable.Range(0, 7)
                .Select(i => since.AddDays(i))
                .Select(date => weeklyRaw.FirstOrDefault(w => w.Date == date)
                    ?? new DailyPostCountDto { Date = date, Count = 0 })
                .ToList();

            var globalPosition = await _context.Communities
                .Where(c => c.IsActive && c.PowerScore > community.PowerScore)
                .CountAsync() + 1;

            // ── XP / Progreso de rango ──
            var allRanks = await _context.CommunityRanks
                .OrderBy(r => r.MinScore)
                .ToListAsync();

            double currentXP = 0;
            double nextLevelXP = 100;
            string nextRankName = "Rango máximo";

            if (community.Rank != null)
            {
                currentXP = community.PowerScore - community.Rank.MinScore;

                var nextRank = allRanks
                    .Where(r => r.MinScore > community.Rank.MinScore)
                    .OrderBy(r => r.MinScore)
                    .FirstOrDefault();

                if (nextRank != null)
                {
                    nextLevelXP = nextRank.MinScore - community.Rank.MinScore;
                    nextRankName = nextRank.Name;
                }
                else
                {
                    nextLevelXP = 100;
                    nextRankName = "Rango máximo";
                }
            }

            // ── Color principal ──
            var mainCategoryColor = community.Categories
                .OrderByDescending(c => c.Id)
                .Select(c => c.ColorHex)
                .FirstOrDefault() ?? "#7c3aed";

            // ── DTO final ──
            var dto = new CommunityDashboardDto
            {
                Id = community.Id,
                Name = community.Name,
                Slug = community.Slug,
                Description = community.Description,
                Country = community.Country,
                ImageUrl = community.ImageUrl,
                BannerUrl = community.BannerUrl,
                Rules = community.Rules,
                Visibility = community.Visibility,
                CreatedAt = community.CreatedAt,

                TotalFollowers = community.UserCommunities.Count,
                TotalPosts = totalPosts,
                TotalComments = totalComments,

                UpVotes = community.UpVotes,
                DownVotes = community.DownVotes,
                PowerScore = community.PowerScore,

                CurrentXP = currentXP,
                NextLevelXP = nextLevelXP,
                NextRankName = nextRankName,
                MainCategoryColor = mainCategoryColor,


                WeeklyNewFollowers = community.WeeklyNewFollowers,
                WeeklyPosts = community.WeeklyPosts,
                WeeklyComments = community.WeeklyComments,


                TopMembers = topMembers,
                CategoryStats = categoryStats,
                Moderators = moderators,
                RecentActivity = recentActivity,
                WeeklyPostChart = weeklyChart,

                RankName = community.Rank != null ? community.Rank.Name : null,
                RankPosition = globalPosition,  // ← posición real, no RankId
                IsDominant = globalPosition == 1,
            };


            // ── Estado del usuario actual ──
            if (currentUserId.HasValue)
            {
                dto.IsFollowing = await _context.Set<UserCommunity>()
                    .AnyAsync(x => x.CommunityId == dto.Id && x.UserId == currentUserId.Value);

                dto.IsOwner = community.CreatedByUserId == currentUserId.Value;

                dto.CurrentUserVote = await _context.Votes
                    .Where(v => v.CommunityId == community.Id && v.UserId == currentUserId.Value)
                    .Select(v => (int?)v.VoteType)
                    .FirstOrDefaultAsync();
            }

            return dto;
        }
    }
}