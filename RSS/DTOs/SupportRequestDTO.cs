namespace RSS.DTOs
{
    // Website is a honeypot — real users never see or fill this field, bots often do
    public record SupportRequestDTO(string Name, string Email, string Category, string Message, string? Website);
}
