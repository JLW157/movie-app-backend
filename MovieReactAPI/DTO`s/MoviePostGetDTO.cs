using MovieReactAPI.Entities;

namespace MovieReactAPI.DTO_s
{
    public class MoviePostGetDTO
    {
        public List<GenreDTO> Genres { get; set; }
        public List<MovieTheaterDTO> MovieTheaters { get; set; }
    }
}
