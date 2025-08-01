namespace SharedEvents.Events;

public record OrderFailedEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}