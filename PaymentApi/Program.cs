using Microsoft.EntityFrameworkCore;
using Payment.Api.Data;
using Payment.Api.Services;
using Payment.Api.BackgroundServices;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<PaymentDbContext>(o =>
    o.UseNpgsql(
        builder.Configuration
        .GetConnectionString("Postgres")));

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("redis:6379"));

builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddHttpClient("gateway", c =>
{
    c.BaseAddress =
        new Uri("http://payment-gateway:8080");
});

builder.Services.AddHostedService<OutboxProcessor>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.MapControllers();

DbInitializer.Initialize(app.Services);

app.Run();