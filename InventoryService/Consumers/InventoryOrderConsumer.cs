using MassTransit;
using Microsoft.Extensions.Logging;
using InventoryService.Models;
using SharedEvents.Events;

namespace InventoryService.Consumers;

public class InventoryOrderConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly InventoryDbContext _dbContext;
    private readonly ILogger<InventoryOrderConsumer> _logger;

    public InventoryOrderConsumer(InventoryDbContext dbContext, ILogger<InventoryOrderConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Processing OrderCreatedEvent for OrderId: {context.Message.OrderId}");
        var product = await _dbContext.Products.FindAsync(context.Message.ProductId);
        if (product != null && product.Stock >= context.Message.Quantity)
        {
            product.Stock -= context.Message.Quantity;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Stock updated for ProductId: {context.Message.ProductId}, New Stock: {product.Stock}");

            _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Publishing InventoryReservedEvent for OrderId: {context.Message.OrderId}");
            await context.Publish(new InventoryReservedEvent { OrderId = context.Message.OrderId });
        }
        else
        {
            _logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Insufficient stock for ProductId: {context.Message.ProductId}");
            await context.Publish(new InventoryReservationFailedEvent
            {
                OrderId = context.Message.OrderId,
                Reason = "Insufficient stock"
            });
        }
    }
}