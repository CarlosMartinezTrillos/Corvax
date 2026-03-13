using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Foro_Militar.Models
{
    public class CreatePostViewModel
    {
        public int CommunityId { get; set; }
        public string CommunitySlug { get; set; }
        public string CommunityName { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [MaxLength(200, ErrorMessage = "Máximo 200 caracteres")]
        public string Title { get; set; }

        [Required(ErrorMessage = "El contenido es obligatorio")]
        public string Content { get; set; }

        [MaxLength(500)]
        public string Image { get; set; }

        [Required(ErrorMessage = "Selecciona una categoría principal")]
        public int MainCategoryId { get; set; }

        public List<int> ExtraCategoryIds { get; set; } = new List<int>();
        public string PostType { get; set; }
        // Categorías de la comunidad — para la sección "Categoría principal"
        public List<CategoryOption> AvailableCategories { get; set; } = new List<CategoryOption>();

        // Todas las categorías de la BD — para la sección "Categorías adicionales"
        public List<CategoryOption> AllCategories { get; set; } = new List<CategoryOption>();
    }

    public class CategoryOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ColorHex { get; set; }
    }
}