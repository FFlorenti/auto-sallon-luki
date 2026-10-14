using System.Net;
using AutoLuki.IntegrationTests.Infrastructure;

namespace AutoLuki.IntegrationTests;

[Collection(IntegrationCollection.Name)]
public class SwaggerTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Api_documentation_is_hidden_by_default()
    {
        var client = postgres.GetFactory("autoluki_swagger_off").CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/openapi/v1.json")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/swagger/index.html")).StatusCode);
    }

    [Fact]
    public async Task Api_documentation_can_be_switched_on_by_configuration()
    {
        var client = postgres
            .GetFactory("autoluki_swagger_on", new Dictionary<string, string?> { ["Swagger:Enabled"] = "true" })
            .CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/openapi/v1.json")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/swagger/index.html")).StatusCode);
    }
}
