namespace Payment.Api.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context =
                scope.ServiceProvider
                .GetRequiredService<PaymentDbContext>();

            context.Database.EnsureCreated();
        }
    }
}