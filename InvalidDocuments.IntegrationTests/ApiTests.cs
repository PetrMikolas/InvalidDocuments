using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.NUnit;
using System.Net;
using System.Text.Json;
using static InvalidDocuments.Server.Endpoints.EndpointsValidationsRegistrationExtensions;

namespace InvalidDocuments.IntegrationTests;

internal class ApiTests
{
    private HttpClient _httpClient;

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    [SetUp]
    public void Setup()
    {
        var factory = new CustomWebApplicationFactory();
        _httpClient = factory.CreateDefaultClient();
    }

    [Test]
    public async Task GetDocumentValidation_DocumentIsInvalid_Success()
    {
        // Arrange 
        var number = "183579AA81";

        var server = new WebHostBuilder()
            .UseKestrel()
            .Configure(app =>
            {
                app.Run(async context =>
                {
                    if (context.Request.Method == HttpMethods.Get && context.Request.GetEncodedPathAndQuery() == $"/dummyapimvcr?dotaz={number}&doklad=0")
                    {
                        await context.Response.WriteAsync("<doklady_neplatne posl_zmena=\"12.8.2010\" pristi_zmeny=\"\">" +
                                                            "<dotaz typ=\"OPs\" cislo=\"183579\" serie=\"AA81\"/>" +
                                                            "<odpoved aktualizovano=\"24.4.2024\" evidovano=\"ano\" evidovano_od=\"15.4.2024\"/>" +
                                                          "</doklady_neplatne>");
                    }
                });
            })
            .Build();

        await server.StartAsync();

        var expectedDto = new DocumentValidationDto
        (
            "183579",
            "AA81",
            "občanský průkaz",
            true,
            "15.4.2024",
            false,
            string.Empty
        );

        // Act
        var response = await _httpClient.GetAsync($"validations?number={number}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNull();

        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        var dto = JsonSerializer.Deserialize<DocumentValidationDto>(content, options)!;
        dto.Should().NotBeNull();
        dto.Should().BeEquivalentTo(expectedDto);
        Snapshot.Match(dto);

        await server.StopAsync();
    }

    [Test]
    public async Task GetDocumentValidation_DocumentIsValid_Success()
    {
        // Arrange 
        var number = "123456ABCD";

        var server = new WebHostBuilder()
            .UseKestrel()
            .Configure(app =>
            {
                app.Run(async context =>
                {
                    if (context.Request.Method == HttpMethods.Get && context.Request.Path == "/dummyapimvcr")
                    {
                        await context.Response.WriteAsync("<doklady_neplatne posl_zmena=\"12.8.2010\" pristi_zmeny=\"\">" +
                                                            "<dotaz typ=\"OPs\" cislo=\"123456\" serie=\"ABCD\"/>" +
                                                            "<odpoved aktualizovano=\"24.4.2024\" evidovano=\"ne\"/>" +
                                                          "</doklady_neplatne>");
                    }
                });
            })
            .Build();

        await server.StartAsync();
    
        var expectedDto = new DocumentValidationDto
        (
            number,
            string.Empty,
            string.Empty,
            false,
            string.Empty,
            false,
            string.Empty
        );

        // Act
        var response = await _httpClient.GetAsync($"validations?number={number}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNull();

        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        var dto = JsonSerializer.Deserialize<DocumentValidationDto>(content, options)!;
        dto.Should().NotBeNull();
        dto.Should().BeEquivalentTo(expectedDto);
        Snapshot.Match(dto);

        await server.StopAsync();
    }

    [Test]
    [TestCase("", HttpStatusCode.BadRequest)]
    [TestCase(null!, HttpStatusCode.BadRequest)]
    [TestCase("12345678ABCD", HttpStatusCode.BadRequest)]
    [TestCase("123456*AB", HttpStatusCode.BadRequest)]
    public async Task GetDocumentValidation_BadRequest(string number, HttpStatusCode expectedStatusCode)
    {
        // Act
        var response = await _httpClient.GetAsync($"validations?number={number}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(expectedStatusCode);
    }
}