namespace SharedEvents.Events;

public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public int CustomerId { get; init; }
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}