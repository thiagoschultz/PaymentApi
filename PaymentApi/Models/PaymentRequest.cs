using System.ComponentModel.DataAnnotations;

namespace Payment.Api.Models
{
    public class PaymentRequest
    {
        [Required]
        public string OrderId { get; set; } = string.Empty;

        [Range(1, 100000)]
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "BRL";
    }
}