using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Models;
using SharedEvents.Events;

namespace OrderService.Consumers;

public class PaymentSuccessConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly OrderDbContext _dbContext;
    private readonly ILogger<PaymentSuccessConsumer> _logger;

    public PaymentSuccessConsumer(OrderDbContext dbContext, ILogger<PaymentSuccessConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Processing PaymentProcessedEvent for OrderId: {context.Message.OrderId}");
        var order = await _dbContext.Orders.FindAsync(context.Message.OrderId);
        if (order != null)
        {
            order.Status = "Completed";
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Order {context.Message.OrderId} updated to status: {order.Status}");

            _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Publishing OrderCompletedEvent for OrderId: {context.Message.OrderId}");
            await context.Publish(new OrderCompletedEvent { OrderId = order.Id });
        }
        else
        {
            _logger.LogWarning($"[{DateTime.Now:HH:mm:ss}] Order {context.Message.OrderId} not found");
        }
    }
}