namespace webapi.Data;

public record AnalyticsEventSummary
{
    public int Count { get; init; }

    public IReadOnlyCollection<AnalyticsEvent> Events { get; init; }
}

public interface IAnalyticsEventSummaryBuilder
{
    AnalyticsEventSummary BuildSummary(IReadOnlyCollection<IAnalyticsEvent> events);
}

public class AnalyticsEventSummaryBuilder : IAnalyticsEventSummaryBuilder
{
    public AnalyticsEventSummary BuildSummary(IReadOnlyCollection<IAnalyticsEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return new AnalyticsEventSummary
        {
            Count = events.Count,
            Events = events
                .Select(e => new AnalyticsEvent
                {
                    Event = e.Event,
                    UserId = e.UserId,
                    Properties = e.Properties
                })
                .ToList()
        };
    }
}
