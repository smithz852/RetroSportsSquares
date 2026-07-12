using System.ComponentModel.DataAnnotations;

namespace RSS.DTOs
{
    public class SendChatMessageDTO
    {
        [Required]
        [MaxLength(500)]
        public string Message { get; set; }
    }
}
