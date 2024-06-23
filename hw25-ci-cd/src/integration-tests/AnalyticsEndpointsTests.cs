using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Refit;
using webapi.Data;

namespace HW25.IntegrationTests;

public class AnalyticsEndpointsTests(IntegrationTestsFixture fixture) : IClassFixture<IntegrationTestsFixture>
{
    [Fact]
    public async Task Insert_StoresData()
    {
        // Arrange
        var client = RestService.For<IEndpoints>(fixture.CreateDefaultClient());

        var analyticsEvent = new AnalyticsEvent
        {
            Event = "test",
            UserId = Guid.NewGuid().ToString(),
            Properties = new Dictionary<string, string>
            {
                { "test", "test" }
            },
        };

        // Act
        var createEventResponse = await client.CreateEvent(analyticsEvent);

        var getSummaryResponse = await client.GetEventsSummary(DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1));

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, createEventResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getSummaryResponse.StatusCode);

        Assert.NotNull(getSummaryResponse.Content);
        Assert.True(getSummaryResponse.Content
                .GetValueOrDefault("MongoDB")?.Events.Any(x => x.UserId == analyticsEvent.UserId),
            "Event not found in summary");
    }
}

public class IntegrationTestsFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseContentRoot(".");
        builder.UseEnvironment("Testing");
        builder.UseSetting("MongoConnection", "mongodb://localhost:27017");

        builder.ConfigureServices(services =>
        {
        });
    }
}

public interface IEndpoints
{
    [Post("/api/v1/analytics/events")]
    Task<IApiResponse> CreateEvent([Body] AnalyticsEvent request);

    [Get("/api/v1/analytics/events/summary")]
    Task<IApiResponse<Dictionary<string, AnalyticsEventSummary>>> GetEventsSummary([Query] DateTime from, [Query] DateTime to);
}
