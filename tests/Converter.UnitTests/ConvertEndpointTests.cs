using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Converter.UnitTests;

public class ConvertEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConvertEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { });
    }

    [Fact]
    public async Task JsonToXml_SpecPasses_ReturnsFile()
    {
        // Arrange
        var client = _factory.CreateClient();
        var req = new
        {
            Id = "TCnWpDVD",
            ReportMetadata = new
            {
                Title = "Document for Publication > Best Report Yet!!",
                ContactSection = new[] {
                    new {
                        ContactInformation = new[] {
                            new {
                                ContactHeader = "Media Contact",
                                Contacts = new[] {
                                    new {
                                        FirstName = "Mike",
                                        LastName = "Johnsen",
                                        Email = "mike.johnsen@kbra.com",
                                        Title = "Director of Communications & Marketing",
                                        PhoneNumber = "+1 646-731-1347",
                                        Accreditation = "CM&AA"
                                    }
                                }
                            }
                        }
                    }
                }
            },
            CountryIds = new[] { "US" },
            Title = "Document for Publication > Best Report Yet!!",
            PublishDate = "2024-08-26T18:19:59Z",
            Status = 3,
            TestRun = true
        };

        // Act
        var res = await client.PostAsJsonAsync("/convert/json-to-xml", req);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("application/xml");
        res.Content.Headers.ContentDisposition!.DispositionType.Should().Be("attachment");
        res.Content.Headers.ContentDisposition!.FileName!
            .Trim('"')
            .EndsWith(".xml", StringComparison.OrdinalIgnoreCase).Should().BeTrue();

        var xml = await res.Content.ReadAsStringAsync();
        xml.Should().Contain("<PublishedItem>");
        xml.Should().Contain("<Title>");
        xml.Should().Contain("<PublishedDate>2024-08-26T18:19:59Z</PublishedDate>");
        xml.Should().Contain("<PersonGroup sequence=\"1\">");
        xml.Should().Contain("<GivenName>Mike</GivenName>");
        xml.Should().Contain("<FamilyName>Johnsen</FamilyName>");
    }

    [Fact]
    public async Task JsonToXml_TestRunFalse_ReturnsBadRequestProblem()
    {
        // Arrange
        var client = _factory.CreateClient();
        var req = new
        {
            Id = "X",
            PublishDate = "2024-08-26T00:00:00Z",
            Status = 3,
            TestRun = false
        };

        // Act
        var res = await client.PostAsJsonAsync("/convert/json-to-xml", req);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("TestRun=true");
    }

    [Fact]
    public async Task JsonToXml_StatusNot3_ReturnsBadRequestProblem()
    {
        // Arrange
        var client = _factory.CreateClient();
        var req = new
        {
            Id = "X",
            PublishDate = "2024-08-26T00:00:00Z",
            Status = 2,
            TestRun = true
        };

        // Act
        var res = await client.PostAsJsonAsync("/convert/json-to-xml", req);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task JsonToXml_PublishDateBeforeCutoff_ReturnsBadRequestProblem()
    {
        // Arrange
        var client = _factory.CreateClient();
        var req = new
        {
            Id = "X",
            PublishDate = "2024-08-23T23:59:59Z",
            Status = 3,
            TestRun = true
        };

        // Act
        var res = await client.PostAsJsonAsync("/convert/json-to-xml", req);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
