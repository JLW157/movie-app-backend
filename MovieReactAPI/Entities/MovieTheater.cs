using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace MovieReactAPI.Entities
{
    public class MovieTheater
    {
        public int Id { get; set; }
        [Required]
        [StringLength(maximumLength:75)]
        public string Name { get; set; }
        public Point Loaction { get; set; }
    }
}
