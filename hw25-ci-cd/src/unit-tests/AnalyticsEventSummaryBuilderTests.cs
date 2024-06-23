using webapi.Data;

namespace HW25.UnitTests;

public class AnalyticsEventSummaryBuilderTests
{
    [Fact]
    public void BuildSummary_ThrowsArgumentNullException_WhenEventsIsNull()
    {
        // Arrange
        var builder = new AnalyticsEventSummaryBuilder();
        
        // Act
        // ReSharper disable once ConvertToLocalFunction
        Action act = () => builder.BuildSummary(null);
        
        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }
    
    [Fact]
    public void BuildSummary_ReturnsSummaryWithCorrectCountAndEvents()
    {
        // Arrange
        var events = new List<IAnalyticsEvent>
        {
            new AnalyticsEvent(),
            new AnalyticsEvent(),
            new AnalyticsEvent()
        };
        
        var builder = new AnalyticsEventSummaryBuilder();
        
        // Act
        var summary = builder.BuildSummary(events);
        
        // Assert
        Assert.Equal(events.Count, summary.Count);
    }
}