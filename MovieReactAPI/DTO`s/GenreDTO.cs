using MovieReactAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace MovieReactAPI.DTO_s
{
    public class GenreDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
