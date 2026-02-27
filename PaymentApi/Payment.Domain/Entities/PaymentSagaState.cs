namespace Payment.Domain.Entities
{
    public class PaymentSagaState
    {

        public Guid Id { get; set; }

        public Guid PaymentId { get; set; }

        public string CurrentState { get; set; }

        public DateTime LastUpdated { get; set; }

    }
}