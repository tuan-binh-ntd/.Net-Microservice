using Play.Common.MongoDB;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Clients;
using Polly;
using Polly.Timeout;
using Play.Common.MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMongo()
    .AddMonogoRepository<InventoryItem>("inventoryitems")
    .AddMonogoRepository<CatalogItem>("catalogitems")
    .AddMassTransitWithRabbitMq();

AddCatalogClient(builder);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddCatalogClient(WebApplicationBuilder builder)
{
    Random jitterer = new();

    builder.Services.AddHttpClient<CatalogClient>(client =>
    {
        client.BaseAddress = new Uri("https://localhost:7051");
    })
        .AddTransientHttpErrorPolicy(options => options.Or<TimeoutRejectedException>().WaitAndRetryAsync(
            5,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                            + TimeSpan.FromMicroseconds(jitterer.Next(0, 1000)),
            onRetry: (outcome, timespan, retryAttempt) =>
            {
                ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
                serviceProvider.GetService<ILogger<CatalogClient>>()?
                    .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
            }
            ))
        .AddTransientHttpErrorPolicy(options => options.Or<TimeoutRejectedException>().CircuitBreakerAsync(
            3,
            TimeSpan.FromSeconds(15),
            onBreak: (outcome, timespan) =>
            {
                ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
                serviceProvider.GetService<ILogger<CatalogClient>>()?
                    .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");
            },
            onReset: () =>
            {
                ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
                serviceProvider.GetService<ILogger<CatalogClient>>()?
                    .LogWarning($"Closing the circuit...");
            }
            ))
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
}