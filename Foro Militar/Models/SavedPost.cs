using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foro.Entities.Models
{
    [Table("SavedPosts")]
    public class SavedPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public int PostId { get; set; }
        public virtual Post Post { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.Now;
    }
}