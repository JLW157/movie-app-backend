using System.ComponentModel.DataAnnotations;

namespace MovieReactAPI.DTO_s
{
    public class ActorCreationDTO
    {
        [Required]
        [StringLength(120)]
        public string Name { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Biography { get; set; } = null!;
        public IFormFile Picture { get; set; } = null!;
    }
}
