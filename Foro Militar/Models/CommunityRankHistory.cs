using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foro.Entities.Models
{
    [Table("CommunityRankHistory")]
    public class CommunityRankHistory
    {
        [Key]
        public int Id { get; set; }

        public int CommunityId { get; set; }
        public int RankId { get; set; }

        public DateTime AchievedAt { get; set; }
        public DateTime? LostAt { get; set; }

        [ForeignKey("CommunityId")]
        public virtual Community Community { get; set; }

        [ForeignKey("RankId")]
        public virtual CommunityRank Rank { get; set; }
    }
}