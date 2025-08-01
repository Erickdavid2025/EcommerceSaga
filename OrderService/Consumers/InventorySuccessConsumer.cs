using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Models;
using SharedEvents.Events;

namespace OrderService.Consumers;

public class InventorySuccessConsumer : IConsumer<InventoryReservedEvent>
{
    private readonly OrderDbContext _dbContext;
    private readonly ILogger<InventorySuccessConsumer> _logger;

    public InventorySuccessConsumer(OrderDbContext dbContext, ILogger<InventorySuccessConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryReservedEvent> context)
    {
        _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Processing InventoryReservedEvent for OrderId: {context.Message.OrderId}");
        var order = await _dbContext.Orders.FindAsync(context.Message.OrderId);
        if (order != null)
        {
            order.Status = "InventoryReserved";
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Order {context.Message.OrderId} updated to status: {order.Status}");

            _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Publishing PaymentProcessedEvent for OrderId: {context.Message.OrderId}");
            await context.Publish(new PaymentProcessedEvent { OrderId = order.Id });
        }
        else
        {
            _logger.LogWarning($"[{DateTime.Now:HH:mm:ss}] Order {context.Message.OrderId} not found");
        }
    }
}