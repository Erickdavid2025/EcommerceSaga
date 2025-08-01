using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Models;
using SharedEvents.Events;

namespace OrderService.Consumers;

public class OrderCreationConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly OrderDbContext _dbContext;
    private readonly ILogger<OrderCreationConsumer> _logger;

    public OrderCreationConsumer(OrderDbContext dbContext, ILogger<OrderCreationConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Processing OrderCreatedEvent for OrderId: {context.Message.OrderId}");
        var order = new Order
        {
            Id = context.Message.OrderId,
            CustomerId = context.Message.CustomerId,
            ProductId = context.Message.ProductId,
            Quantity = context.Message.Quantity,
            Status = "Pending"
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Order {context.Message.OrderId} saved with status: {order.Status}");

        _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Publishing InventoryReservedEvent for OrderId: {context.Message.OrderId}");
        await context.Publish(new InventoryReservedEvent { OrderId = order.Id });
    }
}