using System.ComponentModel.DataAnnotations;

namespace MovieReactAPI.DTO_s
{
    public class MovieTheaterCreationDTO
    {
        [Required]
        [StringLength(maximumLength: 75)]
        public string Name { get; set; }
        [Range(-90, 90)]
        public double Latitude { get; set; }
        [Range(-180, 180)]
        public double Longtiude { get; set; }
    }
}
