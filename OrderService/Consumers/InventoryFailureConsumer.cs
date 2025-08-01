using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Models;
using SharedEvents.Events;

namespace OrderService.Consumers;

public class InventoryFailureConsumer : IConsumer<InventoryReservationFailedEvent>
{
    private readonly OrderDbContext _dbContext;
    private readonly ILogger<InventoryFailureConsumer> _logger;

    public InventoryFailureConsumer(OrderDbContext dbContext, ILogger<InventoryFailureConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryReservationFailedEvent> context)
    {
        _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Processing InventoryReservationFailedEvent for OrderId: {context.Message.OrderId}");
        var order = await _dbContext.Orders.FindAsync(context.Message.OrderId);
        if (order != null)
        {
            order.Status = "Failed";
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Order {context.Message.OrderId} updated to status: {order.Status}");

            _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Publishing OrderFailedEvent for OrderId: {context.Message.OrderId}");
            await context.Publish(new OrderFailedEvent
            {
                OrderId = order.Id,
                Reason = context.Message.Reason
            });
        }
        else
        {
            _logger.LogWarning($"[{DateTime.Now:HH:mm:ss}] Order {context.Message.OrderId} not found");
        }
    }
}