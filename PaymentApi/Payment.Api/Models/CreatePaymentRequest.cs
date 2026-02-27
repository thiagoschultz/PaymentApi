namespace Payment.Api.Models
{
    public class CreatePaymentRequest
    {
        public string OrderId { get; set; }

        public decimal Amount { get; set; }
    }
}