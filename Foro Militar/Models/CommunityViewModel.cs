using System.Collections.Generic;

namespace Foro_Militar.Models
{
    public class CommunityViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public string Country { get; set; }

        public string ImageUrl { get; set; }

        public string BannerUrl { get; set; }

        public int TotalPosts { get; set; }

        public int TotalFollowers { get; set; }

        public string ColorBaseCalculated { get; set; }

        public string CreatedAtFormatted { get; set; }

        public string CreatedByName { get; set; }

        public List<CategoryInfo> Categories { get; set; }

        public string RankName { get; set; }
        public string RankOrderGroup { get; set; }
        public string RankBorderColor { get; set; }
        public string RankGlowColor { get; set; }
        public bool RankHasAnimatedBorder { get; set; }
        public bool RankHasParticleEffect { get; set; }

        public double PowerScore { get; set; }

        public int GlobalPosition { get; set; }
        public int CountryPosition { get; set; }

        public string DisplayGlobalPosition => GlobalPosition == 0 ? "-" : GlobalPosition.ToString();
        public string DisplayCountryPosition => CountryPosition == 0 ? "-" : CountryPosition.ToString();

        public string DisplayPowerScore =>
            PowerScore.ToString("N0"); // Formatea el PowerScore sin decimales y con separadores de miles

        public int Level { get; set; }
        public double CurrentXP { get; set; }
        public double NextLevelXP { get; set; }

        public int LevelProgressPercent =>
            NextLevelXP <= 0 ? 0 :
            (int)((CurrentXP / NextLevelXP) * 100);

        public bool IsNew { get; set; }
        public bool IsTrending { get; set; }
        public bool IsDominant => GlobalPosition == 1;

        public int UpVotes { get; set; }
        public int DownVotes { get; set; }
        public int? CurrentUserVote { get; set; }
        public class CategoryInfo
        {
            public string Name { get; set; }
            public string ColorHex { get; set; }

            public string ColorHexSoft => ColorHex + "22"; // Agrega transparencia al color
        }
    }
}
