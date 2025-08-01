namespace SharedEvents.Events;

public record InventoryReservedEvent
{
    public Guid OrderId { get; init; }
}