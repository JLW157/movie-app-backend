using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MovieReactAPI.Entities
{
    public class Actor
    {
        public int Id { get; set; }
        [Required]
        [StringLength(120)]
        public string Name { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Biography { get; set; } = null!;
        public string Picture { get; set; } = null!;
    }
}
