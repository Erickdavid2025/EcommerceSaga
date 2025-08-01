namespace SharedEvents.Events;

public record PaymentProcessedEvent
{
    public Guid OrderId { get; init; }
}

