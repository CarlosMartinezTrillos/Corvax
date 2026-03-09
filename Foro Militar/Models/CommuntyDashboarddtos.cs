using System;
using System.Collections.Generic;

using System;
using System.Collections.Generic;

namespace Foro.API.DTOs.Community
{
    // ─────────────────────────────────────────
    // RESPONSE PRINCIPAL DEL DASHBOARD
    // ─────────────────────────────────────────

    public class CommunityDashboardDto
    {
        // Info base
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string BannerUrl { get; set; }
        public string Rules { get; set; }
        public int Visibility { get; set; }
        public DateTime CreatedAt { get; set; }

        // Stats strip
        public int TotalFollowers { get; set; }
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public int UpVotes { get; set; }
        public int DownVotes { get; set; }
        public int? CurrentUserVote { get; set; } // 1, -1 o null
        public double PowerScore { get; set; }

        // Weekly deltas (para los "+N esta semana")
        public int WeeklyNewFollowers { get; set; }
        public int WeeklyPosts { get; set; }
        public int WeeklyComments { get; set; }

        // Rank
        public string RankName { get; set; }
        public int? RankPosition { get; set; }

        // Sidebar widgets
        public List<TopMemberDto> TopMembers { get; set; }
        public List<CategoryStatsDto> CategoryStats { get; set; }
        public List<ModeratorDto> Moderators { get; set; }
        public List<RecentActivityDto> RecentActivity { get; set; }
        public List<DailyPostCountDto> WeeklyPostChart { get; set; }

        // Estado del usuario actual
        public bool IsFollowing { get; set; }
        public bool IsModerator { get; set; }
        public bool IsOwner { get; set; }
    }

    // ─────────────────────────────────────────
    // SIDEBAR: TOP MIEMBROS
    // ─────────────────────────────────────────

    public class TopMemberDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public int Rank { get; set; } // posición 1,2,3...
    }

    // ─────────────────────────────────────────
    // SIDEBAR: STATS POR CATEGORÍA
    // ─────────────────────────────────────────

    public class CategoryStatsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ColorHex { get; set; }
        public int PostCount { get; set; }
        public double Percentage { get; set; } // relativo al total de posts
    }

    // ─────────────────────────────────────────
    // SIDEBAR: MODERADORES
    // ─────────────────────────────────────────

    public class ModeratorDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public bool IsOwner { get; set; }
    }

    // ─────────────────────────────────────────
    // SIDEBAR: ACTIVIDAD RECIENTE
    // ─────────────────────────────────────────

    public class RecentActivityDto
    {
        public string Type { get; set; }     // "post" | "comment" | "join" | "vote"
        public string Username { get; set; }
        public string Summary { get; set; }  // ej: "publicó 'Batalla de Midway...'"
        public DateTime CreatedAt { get; set; }
    }

    // ─────────────────────────────────────────
    // SIDEBAR: GRÁFICO SEMANAL
    // ─────────────────────────────────────────

    public class DailyPostCountDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    // ─────────────────────────────────────────
    // FEED DE POSTS (paginado, separado)
    // ─────────────────────────────────────────

    public class PostFeedDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ContentExcerpt { get; set; } // primeros 300 chars
        public string Image { get; set; }

        public int UserId { get; set; }
        public string Username { get; set; }

        public int UpVotes { get; set; }
        public int DownVotes { get; set; }
        public int Score { get; set; }       // upvotes - downvotes
        public int CommentCount { get; set; }

        // Ranking visual
        public bool IsDominant { get; set; }

        public string MainCategoryName { get; set; }
        public string MainCategoryColor { get; set; }
        public List<string> ExtraCategories { get; set; }

        public bool IsPinned { get; set; }
        public int? CurrentUserVote { get; set; } // 1, -1 o null

        public DateTime CreatedAt { get; set; }
        public double HotScore { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage => Page * PageSize < TotalCount;
    }
} 