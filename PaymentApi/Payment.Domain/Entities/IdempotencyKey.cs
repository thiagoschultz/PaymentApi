namespace Payment.Domain.Entities
{
    public class IdempotencyKey
    {
        public Guid Id { get; set; }

        public string Key { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}