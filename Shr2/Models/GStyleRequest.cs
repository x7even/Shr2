using System.ComponentModel.DataAnnotations;

namespace Shr2.Models
{
    public class GStyleRequest
    {
        [Required]
        [Url]
        public string LongUrl { get; set; } = string.Empty;
    }
}
