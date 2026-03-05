using System;
using System.Linq;
using Foro.Entities.Models;

namespace Foro_Militar.Services
{
    public class VoteService
    {
        private readonly AppDbContext _context;

        public VoteService(AppDbContext context)
        {
            _context = context;
        }


        public int VoteCommunity(int communityId, int userId, int voteType)
        {
            if (voteType != 1 && voteType != -1)
                throw new ArgumentException("Invalid vote type");

            var communityExists = _context.Communities.Any(c => c.Id == communityId);
            if (!communityExists)
                throw new Exception("Community not found");

            var existingVotes = _context.Votes
                .Where(v => v.CommunityId == communityId && v.UserId == userId)
                .ToList();

            // 🔥 Si por error hay más de uno, limpia
            if (existingVotes.Count > 1)
            {
                _context.Votes.RemoveRange(existingVotes.Skip(1));
                _context.SaveChanges();
            }

            var existingVote = existingVotes.FirstOrDefault();

            if (existingVote != null)
            {
                if (existingVote.VoteType == voteType)
                    _context.Votes.Remove(existingVote);
                else
                    existingVote.VoteType = voteType;
            }
            else
            {
                _context.Votes.Add(new Vote
                {
                    CommunityId = communityId,
                    UserId = userId,
                    VoteType = voteType,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.SaveChanges();

            var community = _context.Communities
                .Include("Votes")
                .First(c => c.Id == communityId);

            community.UpVotes = community.Votes.Count(v => v.VoteType == 1);
            community.DownVotes = community.Votes.Count(v => v.VoteType == -1);

            _context.SaveChanges();

            return community.UpVotes - community.DownVotes;
        }

        public int GetCommunityVoteCount(int communityId)
        {
            return _context.Votes
                .Where(v => v.CommunityId == communityId)
                .Sum(v => (int?)v.VoteType) ?? 0;
        }

        // Para Posts
        public int VotePost(int postId, int userId, int voteType)
        {
            var existingVote = _context.Votes
                .FirstOrDefault(v =>
                    v.PostId == postId &&
                    v.UserId == userId);

            if (existingVote != null)
            {
                if (existingVote.VoteType == voteType)
                {
                    _context.Votes.Remove(existingVote);
                }
                else
                {
                    existingVote.VoteType = voteType;
                }
            }
            else
            {
                _context.Votes.Add(new Vote
                {
                    PostId = postId,
                    UserId = userId,
                    VoteType = voteType,
                    CreatedAt = DateTime.Now
                });
            }

            _context.SaveChanges();

            return GetPostVoteCount(postId);
        }

        public int GetPostVoteCount(int postId)
        {
            return _context.Votes
                .Where(v => v.PostId == postId)
                .Sum(v => (int?)v.VoteType) ?? 0;
        }
    }
}