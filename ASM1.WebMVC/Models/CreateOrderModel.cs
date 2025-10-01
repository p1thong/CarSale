using System.ComponentModel.DataAnnotations;

namespace ASM1.WebMVC.Models
{
    public class CreateOrderModel
    {
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public int VariantId { get; set; }
        
        public string? Notes { get; set; }
        
        public DateOnly? DeliveryDate { get; set; }
        
        public string Priority { get; set; } = "Normal";
    }
}