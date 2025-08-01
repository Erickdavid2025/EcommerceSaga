namespace SharedEvents.Events;

public record OrderCompletedEvent
{
    public Guid OrderId { get; init; }
}