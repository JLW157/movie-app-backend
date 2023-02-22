namespace MovieReactAPI.DTO_s
{
    public class ActorDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Biography { get; set; } = null!;
        public string Picture { get; set; } = null!;
    }
}
