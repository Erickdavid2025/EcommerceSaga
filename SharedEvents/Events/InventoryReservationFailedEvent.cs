namespace SharedEvents.Events;

public record InventoryReservationFailedEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}