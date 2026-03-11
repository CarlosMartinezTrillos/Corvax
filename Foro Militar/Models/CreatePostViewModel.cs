using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Foro_Militar.Models
{
    public class CreatePostViewModel
    {
        // Contexto de la comunidad (se pasa desde el GET)
        public int CommunityId { get; set; }
        public string CommunitySlug { get; set; }
        public string CommunityName { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [MaxLength(200, ErrorMessage = "Máximo 200 caracteres")]
        public string Title { get; set; }

        [Required(ErrorMessage = "El contenido es obligatorio")]
        public string Content { get; set; }

        [MaxLength(500)]
        public string Image { get; set; }   // URL opcional

        [Required(ErrorMessage = "Selecciona una categoría principal")]
        public int MainCategoryId { get; set; }

        // Categorías adicionales (opcional)
        public List<int> ExtraCategoryIds { get; set; } = new List<int>();

        // Para poblar el select/checkboxes en la vista
        public List<CategoryOption> AvailableCategories { get; set; } = new List<CategoryOption>();
    }

    public class CategoryOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ColorHex { get; set; }
    }
}