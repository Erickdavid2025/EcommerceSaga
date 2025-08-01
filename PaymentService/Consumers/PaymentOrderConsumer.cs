using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentService.Models;
using SharedEvents.Events;

namespace PaymentService.Consumers;

public class PaymentOrderConsumer : IConsumer<InventoryReservedEvent>
{
    private readonly PaymentDbContext _dbContext;
    private readonly ILogger<PaymentOrderConsumer> _logger;

    public PaymentOrderConsumer(PaymentDbContext dbContext, ILogger<PaymentOrderConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryReservedEvent> context)
    {
        _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Processing InventoryReservedEvent for OrderId: {context.Message.OrderId}");
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = context.Message.OrderId,
            CustomerId = 1, // Simulado
            Amount = 100.00m, // Simulado
            Status = "Processed"
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Payment {payment.Id} saved for OrderId: {context.Message.OrderId}");

        bool paymentSuccessful = true; // Cambia a false para simular fallo
        if (paymentSuccessful)
        {
            _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Publishing PaymentProcessedEvent for OrderId: {context.Message.OrderId}");
            await context.Publish(new PaymentProcessedEvent { OrderId = context.Message.OrderId });
        }
        else
        {
            _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Publishing PaymentFailedEvent for OrderId: {context.Message.OrderId}");
            await context.Publish(new PaymentFailedEvent
            {
                OrderId = context.Message.OrderId,
                Reason = "Payment declined"
            });
        }
    }
}