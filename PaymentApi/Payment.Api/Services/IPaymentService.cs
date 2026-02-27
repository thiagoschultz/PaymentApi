namespace Payment.Api.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePaymentAsync(
            PaymentRequest request,
            string idempotencyKey);

        Task<PaymentResponse?> GetPaymentAsync(Guid id);

        Task<IEnumerable<PaymentResponse>> GetPaymentsAsync();

        Task<bool> CancelPaymentAsync(Guid id);
    }
}