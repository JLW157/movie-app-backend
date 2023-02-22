using System.ComponentModel.DataAnnotations;

namespace MovieReactAPI.DTO_s
{
    public class MovieTheaterDTO
    {
        public int Id { get; set; }
        [Required]
        [StringLength(maximumLength: 75)]
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longtiude { get; set; }
    }
}
