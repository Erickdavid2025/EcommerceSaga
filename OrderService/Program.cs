using Microsoft.EntityFrameworkCore;
using MassTransit;
using OrderService.Models;
using SharedEvents.Events;
using OrderService.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar MassTransit con RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreationConsumer>();
    x.AddConsumer<InventorySuccessConsumer>();
    x.AddConsumer<InventoryFailureConsumer>();
    x.AddConsumer<PaymentSuccessConsumer>();
    x.AddConsumer<PaymentFailureConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        cfg.ReceiveEndpoint("order-service", e =>
        {
            e.ConfigureConsumer<OrderCreationConsumer>(context);
            e.ConfigureConsumer<InventorySuccessConsumer>(context);
            e.ConfigureConsumer<InventoryFailureConsumer>(context);
            e.ConfigureConsumer<PaymentSuccessConsumer>(context);
            e.ConfigureConsumer<PaymentFailureConsumer>(context);
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();
app.MapControllers();

// Crear la base de datos
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();