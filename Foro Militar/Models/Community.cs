using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foro.Entities.Models
{
    [Table("Communities")]
    public class Community
    {
        public Community()
        {
            Posts = new HashSet<Post>();
            UserCommunities = new HashSet<UserCommunity>();
            Categories = new HashSet<Category>();
            RankHistory = new HashSet<CommunityRankHistory>();
        }

        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }
        public virtual User CreatedByUser { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public bool IsActive { get; set; } = true;

        [MaxLength(100)]
        public string Country { get; set; }

        [MaxLength(150)]
        [Required]
        public string Slug { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(500)]
        public string BannerUrl { get; set; }

        public int? MainCategoryId { get; set; }
        public virtual Category MainCategory { get; set; }

        [MaxLength(1000)]
        public string Rules { get; set; }

        [Required]
        public int Visibility { get; set; } = 0;

        public int TotalComments { get; set; }
        public int UpVotes { get; set; }

        public int DownVotes { get; set; }

        public int WeeklyNewFollowers { get; set; }
        public int WeeklyPosts { get; set; }
        public int WeeklyComments { get; set; }

        public int PowerScore { get; set; }
        public int? RankId { get; set; }

        [ForeignKey("RankId")]
        public virtual CommunityRank Rank { get; set; }

        public virtual ICollection<CommunityRankHistory> RankHistory { get; set; }

        public class CommunityVote
        {
            public int Id { get; set; }
            public int CommunityId { get; set; }
            public string UserId { get; set; }
            public bool IsUpvote { get; set; }

            public virtual Community Community { get; set; }
        }

        public DateTime? LastActivityAt { get; set; }
        // Relaciones
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<UserCommunity> UserCommunities { get; set; }

        [InverseProperty("Community")]
        public virtual ICollection<Category> Categories { get; set; }


    }
}