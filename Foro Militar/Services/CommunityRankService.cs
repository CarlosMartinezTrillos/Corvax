using Foro.Entities.Models;
using System;
using System.Data.Entity;
using System.Linq;

namespace Foro_Militar.Services
{
    public class CommunityRankService
    {
        private readonly AppDbContext _context;

        public CommunityRankService(AppDbContext context)
        {
            _context = context;
        }

        public void RecalculateRank(Community community)
        {
            var activityScore = CalculateActivityScore(community);
            var weeklyGrowthScore = CalculateWeeklyGrowthScore(community);

            var powerScore =
                (community.UserCommunities.Count * 2.5)
                + (community.Posts.Count * 1.8)
                + (community.TotalComments * 1.2)
                + ((community.UpVotes - community.DownVotes) * 3)
                + (activityScore * 4)
                + (weeklyGrowthScore * 5);

            var newRank = _context.CommunityRanks
                .Where(r => powerScore >= r.MinScore)
                .OrderByDescending(r => r.MinScore)
                .FirstOrDefault();

            if (newRank == null)
                return;

            if (community.RankId != newRank.Id)
            {
                HandleRankChange(community, newRank);
            }

            community.PowerScore = powerScore;
            community.RankId = newRank.Id;
        }

        private double CalculateActivityScore(Community c)
        {
            var lastActivity = c.LastActivityAt ?? DateTime.Now;
            var daysInactive = (DateTime.Now - lastActivity).TotalDays;

            if (daysInactive <= 1) return 10;
            if (daysInactive <= 3) return 7;
            if (daysInactive <= 7) return 4;
            return 1;
        }

        private double CalculateWeeklyGrowthScore(Community c)
        {
            return (c.WeeklyNewFollowers * 2)
                   + (c.WeeklyPosts * 1.5)
                   + (c.WeeklyComments * 1.2);
        }

        private void HandleRankChange(Community community, CommunityRank newRank)
        {
            var currentHistory = _context.CommunityRankHistories
                .FirstOrDefault(h => h.CommunityId == community.Id && h.LostAt == null);

            if (currentHistory != null)
                currentHistory.LostAt = DateTime.Now;

            _context.CommunityRankHistories.Add(new CommunityRankHistory
            {
                CommunityId = community.Id,
                RankId = newRank.Id,
                AchievedAt = DateTime.Now
            });
        }
    }
}