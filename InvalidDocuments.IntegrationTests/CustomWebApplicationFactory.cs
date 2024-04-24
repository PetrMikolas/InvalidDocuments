using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InvalidDocuments.IntegrationTests;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    internal HttpClient CreateDefaultClient()
    {
        var client = WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("IntegrationTests");

        }).CreateClient();

        return client;
    }
}