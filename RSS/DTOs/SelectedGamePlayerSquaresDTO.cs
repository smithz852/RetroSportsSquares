namespace RSS.DTOs
{
    public class SelectedGamePlayerSquaresDTO
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string SquareId { get; set; }
        public DateTimeOffset SelectedAt { get; set; }
    }
}
