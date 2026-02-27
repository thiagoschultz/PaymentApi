namespace Payment.Api.Models
{
    public class Payment
    {
        public Guid Id { get; set; }

        public string OrderId { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "BRL";

        public PaymentStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}