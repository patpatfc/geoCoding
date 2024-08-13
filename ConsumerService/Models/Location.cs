using System.ComponentModel.DataAnnotations;

namespace ConsumerService.Models
{
    public class Location
    {
        [Required]
        public double latitude { get; set; }
        
        [Required]
        public double longitude { get; set; }
    }
}