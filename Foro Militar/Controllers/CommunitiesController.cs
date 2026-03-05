using Foro.Entities.Models;
using Foro_Militar.Models;
using Foro_Militar.Services;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace Foro_Militar.Controllers
{
    public class CommunitiesController : Controller
    {
        private readonly AppDbContext _context = new AppDbContext();
        private readonly VoteService _voteService;

        public CommunitiesController()
        {
            _voteService = new VoteService(_context);
        }



        // GET: /Communities
        public ActionResult Index()
        {
            int userId = 0;
            if (User.Identity.IsAuthenticated)
                userId = int.Parse(User.Identity.Name);
            var rankService = new CommunityRankService(_context);


            var communities = _context.Communities
                .Include(c => c.Posts)
                .Include(c => c.UserCommunities)
                .Include(c => c.Categories)
                .Include(c => c.MainCategory)
                .Include(c => c.CreatedByUser)
                .Include(c => c.Rank)
                .Include(c => c.Votes)
                .ToList();

            foreach (var c in communities)
            {
                rankService.RecalculateRank(c);
            }

            var globalRanking = communities
                .OrderByDescending(c => c.PowerScore)
                .Select((c, index) => new
                {
                    CommunityId = c.Id,
                    Position = index + 1
                })
                .ToDictionary(x => x.CommunityId, x => x.Position);

            var countryRanking = communities
                .GroupBy(c => c.Country)
                .SelectMany(group =>
                    group.OrderByDescending(c => c.PowerScore)
                         .Select((c, index) => new
                         {
                             CommunityId = c.Id,
                             Position = index + 1
                         }))
                .ToDictionary(x => x.CommunityId, x => x.Position);

            var model = communities.Select(c => new CommunityViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Country = c.Country,
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                BannerUrl = c.BannerUrl,

                CreatedAtFormatted = c.CreatedAt.ToString("MMM yyyy"),
                CreatedByName = c.CreatedByUser.Username,

                TotalPosts = c.Posts.Count,
                TotalFollowers = c.UserCommunities.Count,

                RankName = c.Rank?.Name,
                RankOrderGroup = c.Rank?.OrderGroup,
                RankBorderColor = c.Rank?.BorderColor,
                RankGlowColor = c.Rank?.GlowColor,
                RankHasAnimatedBorder = c.Rank?.HasAnimatedBorder ?? false,
                RankHasParticleEffect = c.Rank?.HasParticleEffect ?? false,
                GlobalPosition = globalRanking.ContainsKey(c.Id) ? globalRanking[c.Id] : 0,
                CountryPosition = countryRanking.ContainsKey(c.Id) ? countryRanking[c.Id] : 0,
                PowerScore = c.PowerScore,

                UpVotes = c.Votes.Count(v => v.VoteType == 1),

                DownVotes = c.Votes.Count(v => v.VoteType == -1),

                CurrentUserVote = c.Votes
                    .Where(v => v.UserId == userId)
                    .Select(v => (int?)v.VoteType)
                    .FirstOrDefault(),

                IsFollowing = c.UserCommunities.Any(uc => uc.UserId == userId),

                Categories = c.Categories.Select(cat => new CommunityViewModel.CategoryInfo
                {
                    Name = cat.Name,
                    ColorHex = cat.ColorHex
                }).ToList(),

                ColorBaseCalculated = c.MainCategory != null
                    ? c.MainCategory.ColorHex
                    : "#7c3aed"
            }).ToList();

            return View(model);
        }

        // GET: /Communities/Details/5
        public ActionResult Details(int id)
        {
            var community = _context.Communities
                .Include(c => c.Posts)
                .Include(c => c.UserCommunities)
                .Include(c => c.Categories)
                .FirstOrDefault(c => c.Id == id);

            if (community == null)
                return HttpNotFound();

            var model = new CommunityViewModel
            {
                Id = community.Id,
                Name = community.Name,
                Description = community.Description,
                Country = community.Country,
                TotalPosts = community.Posts.Count,
                TotalFollowers = community.UserCommunities.Count,
                Categories = community.Categories.Select(cat => new CommunityViewModel.CategoryInfo
                {
                    Name = cat.Name,
                    ColorHex = cat.ColorHex
                }).ToList(),
                ColorBaseCalculated = community.Categories.Any()
                    ? community.Categories.FirstOrDefault().ColorHex
                    : "#7c3aed"
            };

            return View(model);

        }
        public ActionResult RunWeeklyRankJob()
        {
            var job = new Foro_Militar.Services.Jobs.WeeklyCommunityRankJob(_context);
            job.Execute();

            return Content("Weekly rank recalculated successfully.");
        }
        [HttpPost]
        [Authorize]
        public JsonResult VoteCommunity(int id, int voteType)
        {
            int userId = int.Parse(User.Identity.Name);

            _voteService.VoteCommunity(id, userId, voteType);

            var community = _context.Communities
                .Include("Votes")
                .First(c => c.Id == id);

            var up = community.Votes.Count(v => v.VoteType == 1);
            var down = community.Votes.Count(v => v.VoteType == -1);

            return Json(new
            {
                success = true,
                upVotes = up,
                downVotes = down
            });


        }
        [HttpPost]
        [Authorize]
        public JsonResult ToggleFollowCommunity(int communityId)  // <-- cambia 'id' por 'communityId'
        {
            int currentUserId = int.Parse(User.Identity.Name); // <-- cambia 'userId' por 'currentUserId'

            var existing = _context.UserCommunities
                                   .FirstOrDefault(uc => uc.UserId == currentUserId && uc.CommunityId == communityId);

            bool nowFollowing;

            if (existing == null)
            {
                _context.UserCommunities.Add(new UserCommunity
                {
                    UserId = currentUserId,
                    CommunityId = communityId
                });
                nowFollowing = true;
            }
            else
            {
                _context.UserCommunities.Remove(existing);
                nowFollowing = false;
            }

            _context.SaveChanges();

            var totalFollowers = _context.UserCommunities.Count(uc => uc.CommunityId == communityId);

            return Json(new
            {
                success = true,
                isFollowing = nowFollowing,
                totalFollowers
            });

        }
    }
}
