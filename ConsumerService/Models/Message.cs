using System.ComponentModel.DataAnnotations;

namespace ConsumerService.Models
{
    public class Message
    {
        [Required]
        public System.ReadOnlyMemory<byte> Body { get; set; }
        
        [Required]
        public ulong DeliveryTag { get; set; }
    }
}