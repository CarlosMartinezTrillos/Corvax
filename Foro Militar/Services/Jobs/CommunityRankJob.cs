using Foro.Entities.Models;
using Foro_Militar.Services;
using System.Data.Entity;
using System.Linq;

namespace Foro_Militar.Services.Jobs
{
    public class WeeklyCommunityRankJob
    {
        private readonly AppDbContext _context;

        public WeeklyCommunityRankJob(AppDbContext context)
        {
            _context = context;
        }

        public void Execute()
        {
            var rankService = new CommunityRankService(_context);

            var communities = _context.Communities
                .Include(c => c.Posts)
                .Include(c => c.UserCommunities)
                .ToList();

            foreach (var c in communities)
            {
                rankService.RecalculateRank(c);

                // Reset semanal
                c.WeeklyNewFollowers = 0;
                c.WeeklyPosts = 0;
                c.WeeklyComments = 0;
            }

            _context.SaveChanges();
        }
    }
}