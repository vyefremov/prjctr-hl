namespace webapi.Data;

public interface IAnalyticsEventStore
{
    string Name { get; }
    
    Task InsertAsync(IAnalyticsEvent analyticsEvent);

    Task<IReadOnlyCollection<IAnalyticsEvent>> GetLatestAsync(DateTime dateFrom, DateTime dateTo);
}

public interface IAnalyticsEvent
{
    string Event { get; }
    string UserId { get; }
    IReadOnlyDictionary<string, string> Properties { get; }
    DateTime Timestamp { get; }
}

public class AnalyticsEvent : IAnalyticsEvent
{
    public string Event { get; init; }
    public string UserId { get; init; }
    public IReadOnlyDictionary<string, string> Properties { get; init; }
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
