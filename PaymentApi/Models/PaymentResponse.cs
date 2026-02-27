namespace Payment.Api.Models
{
    public class PaymentResponse
    {
        public Guid Id { get; set; }

        public string OrderId { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}