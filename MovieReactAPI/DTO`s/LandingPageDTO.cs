namespace MovieReactAPI.DTO_s
{
    public class LandingPageDTO
    {
        public List<MovieDTO> InTheaters { get; set; } = null!;
        public List<MovieDTO> UpcomingReleases { get; set; } = null!;
    }
}
