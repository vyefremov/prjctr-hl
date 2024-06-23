using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
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
        var response = await client.CreateEvent(analyticsEvent);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        fixture.AnalyticsEventStoreMock
            .Verify(mock => mock.InsertAsync(It.Is<AnalyticsEvent>(x => x.UserId == analyticsEvent.UserId)),
                Times.Once);
    }
}

public class IntegrationTestsFixture : WebApplicationFactory<Program>
{
    public Mock<IAnalyticsEventStore> AnalyticsEventStoreMock { get; } = new();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseContentRoot(".");
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(AnalyticsEventElasticStore));
            services.RemoveAll(typeof(AnalyticsEventMongoStore));
            services.RemoveAll(typeof(ElasticStoreInitializer));
            services.RemoveAll(typeof(MongoStoreInitializer));

            services.AddSingleton(AnalyticsEventStoreMock.Object);
        });
    }
}

public interface IEndpoints
{
    [Post("/analytics/events")]
    Task<IApiResponse> CreateEvent([Body] AnalyticsEvent request);
}
