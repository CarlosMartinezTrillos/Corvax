using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foro.Entities.Models
{
    [Table("CommunityRanks")]
    public class CommunityRank
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string OrderGroup { get; set; }
        public int Level { get; set; }
        public double MinScore { get; set; }

        public string BorderColor { get; set; }
        public string GlowColor { get; set; }

        public bool HasAnimatedBorder { get; set; }
        public bool HasBackgroundEffect { get; set; }
        public bool HasCrownIcon { get; set; }
        public bool HasParticleEffect { get; set; }
    }
}