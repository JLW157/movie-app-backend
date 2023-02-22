using MovieReactAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace MovieReactAPI.Entities
{
    public class Genre
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The field with name is required")]
        [StringLength(50, ErrorMessage = "This field has max count of chars is 50")]
        [FirstLetterUppercase]
        public string Name { get; set; }
    }
}
