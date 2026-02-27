namespace Payment.Api.Models
{
    public class IPaymentService
    {
        public string OrderId { get; set; }

        public decimal Amount { get; set; }
    }
}