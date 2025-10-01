using System.ComponentModel.DataAnnotations;

namespace ASM1.WebMVC.Models
{
    public class TestDriveRequestModel
    {
        public int? CustomerId { get; set; }
        public int VariantId { get; set; }
        public DateOnly ScheduledDate { get; set; }
        public string? PreferredTime { get; set; }
        
        // Guest user information
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        public string? GuestPhone { get; set; }
        public string? GuestLicense { get; set; }
        public DateOnly? GuestBirthday { get; set; }
        
        // Additional notes
        public string? Notes { get; set; }
    }
}