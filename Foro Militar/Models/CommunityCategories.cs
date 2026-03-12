using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foro.Entities.Models
{
    [Table("CommunityCategories")]
    public class CommunityCategory
    {
        [Key, Column(Order = 0)]
        public int CommunityId { get; set; }
        public virtual Community Community { get; set; }

        [Key, Column(Order = 1)]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public bool IsMain { get; set; } = false;
    }
}