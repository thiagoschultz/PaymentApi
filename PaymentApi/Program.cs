using Microsoft.EntityFrameworkCore;
using Payment.Application.Services;
using Payment.Infrastructure.Data;
using Serilog;
using Polly;
using OpenTelemetry.Trace;
using Prometheus;


var builder = WebApplication.CreateBuilder(args);


// =========================
// LOGS (Serilog)
// =========================

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();


// =========================
// Controllers
// =========================

builder.Services.AddControllers();


// =========================
// Swagger
// =========================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();


// =========================
// SQL SERVER
// =========================

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration
        .GetConnectionString("sql")));


// =========================
// REDIS ***********|CREATE BY SCHULTZ IT|*************
// =========================

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis:6379";
});


// =========================
// SERVICES ***********|CREATE BY SCHULTZ IT|*************
// =========================

builder.Services.AddScoped<PaymentService>();


// =========================
// MEDIATR (CQRS) ***********|CREATE BY SCHULTZ IT|*************
// =========================

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(PaymentService).Assembly));


// ========================= 
// HTTP CLIENT GATEWAY ***********|CREATE BY SCHULTZ IT|*************
// =========================

builder.Services.AddHttpClient("gateway", client =>
{
    client.BaseAddress =
        new Uri("http://gatewaySimulator");
})
.AddTransientHttpErrorPolicy(p =>
p.WaitAndRetryAsync(
5,
retry =>
TimeSpan.FromSeconds(
Math.Pow(2, retry))))
.AddPolicyHandler(

Policy.Handle<HttpRequestException>()

.CircuitBreakerAsync(
5,
TimeSpan.FromSeconds(30)));



// =========================
// HEALTH CHECKS ***********|CREATE BY SCHULTZ IT|*************
// =========================

builder.Services.AddHealthChecks()
.AddSqlServer(
builder.Configuration.GetConnectionString("sql"))
.AddRedis("redis:6379");


// =========================
// OPENTELEMETRY ***********|CREATE BY SCHULTZ IT|*************
// =========================

builder.Services.AddOpenTelemetry()

.WithTracing(b => b

.AddAspNetCoreInstrumentation()

.AddHttpClientInstrumentation()

.AddConsoleExporter());



var app = builder.Build();

app.UseSwagger();
app.UseSwagger();
app.UseRouting();
app.UseAuthorization();

// =========================
// PROMETHEUS METRICS
// =========================

app.UseHttpMetrics();

// =========================
// ENDPOINTS
// =========================

app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics();
app.Run();