using System.ComponentModel.DataAnnotations;

namespace MovieReactAPI.DTO_s
{
    public class UserCredentionals
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
