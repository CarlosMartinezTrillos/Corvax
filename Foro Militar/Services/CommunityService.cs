using Foro.API.DTOs.Community;
using Foro.Entities.Models;
using System.Data.Entity;
using System.Linq;

namespace Foro_Militar.Services
{
    public class CommunityService
    {
        private readonly AppDbContext _context;

        public CommunityService(AppDbContext context)
        {
            _context = context;
        }

        public CommunityDashboardDto GetDashboard(string slug)
        {
            var community = _context.Communities
                .Include(c => c.CreatedByUser)
                .Include(c => c.UserCommunities)
                .Include(c => c.Posts)
                .FirstOrDefault(c => c.Slug == slug);

            if (community == null)
                return null;

            var dto = new CommunityDashboardDto
            {
                Id = community.Id,
                Name = community.Name,
                Slug = community.Slug,
                Description = community.Description,
                ImageUrl = community.ImageUrl,
                BannerUrl = community.BannerUrl,

                TotalFollowers = community.UserCommunities.Count,
                TotalPosts = community.Posts.Count,

                CreatedAt = community.CreatedAt,
                UpVotes = community.UpVotes,
                DownVotes = community.DownVotes,
                PowerScore = community.PowerScore
            };

            return dto;
        }
    }
}