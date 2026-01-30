namespace RSS.DTOs
{
    public class CreateGameDTO
    {
        public string Name { get; set; }
        public string GameType { get; set; }
        public string Status { get; set; }
        public int PlayerCount { get; set; }
    }
}