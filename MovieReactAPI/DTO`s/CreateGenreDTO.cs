using MovieReactAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace MovieReactAPI.DTO_s
{
    public class CreateGenreDTO
    {
        [Required(ErrorMessage = "The field with name is required")]
        [StringLength(50, ErrorMessage = "This field has max count of chars is 50")]
        [FirstLetterUppercase]
        public string Name { get; set; } = null!;
    }
}
