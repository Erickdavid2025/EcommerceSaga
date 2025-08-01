using Microsoft.AspNetCore.Mvc;
using MassTransit;
using SharedEvents.Events;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;

    public OrdersController(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var orderId = Guid.NewGuid();
        await _publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = orderId,
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            Quantity = request.Quantity
        });

        return Ok(new { OrderId = orderId });
    }
}

public record CreateOrderRequest(int CustomerId, int ProductId, int Quantity);