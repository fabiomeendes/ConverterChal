using System.Net;
using System.Net.Http.Json;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Converter.UnitTests;

public class ConvertEndpoint_MultiContactsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConvertEndpoint_MultiContactsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { });
    }

    private static object BuildPayload() => new
    {
        Id = "TCnWpDVD",
        ReportMetadata = new
        {
            Title = "Document for Publication > Best Report Yet!!",
            ContactSection = new[]
            {
                new
                {
                    ContactInformation = new object[]
                    {
                        new
                        {
                            ContactHeader = "Media Contact",
                            Contacts = new object[]
                            {
                                new
                                {
                                    FirstName = "Mike",
                                    LastName = "Johnsen",
                                    Email = "mike.johnsen@kbra.com",
                                    Title = "Director of Communications & Marketing",
                                    PhoneNumber = "+1 646-731-1347",
                                    Accreditation = "CM&AA"
                                },
                                new
                                {
                                    FirstName = "",
                                    LastName = "",
                                    Email = "",
                                    Title = "",
                                    PhoneNumber = "",
                                    Accreditation = ""
                                }
                            }
                        },
                        new
                        {
                            ContactHeader = "Analytical Contacts",
                            Contacts = new object[]
                            {
                                new
                                {
                                    FirstName = "Fabio",
                                    LastName = "Camillo",
                                    Email = "fabiomcamillo@gmail.com",
                                    Title = "CTO",
                                    PhoneNumber = "+1 777-731-1347",
                                    Accreditation = "aaaa"
                                },
                                new
                                {
                                    FirstName = "Fabio 2",
                                    LastName = "Camillo 2",
                                    Email = "fabiomcamillo2@gmail.com",
                                    Title = "CTO2",
                                    PhoneNumber = "+1 777-888-1347",
                                    Accreditation = "bbbb"
                                }
                            }
                        }
                    }
                }
            }
        },
        CountryIds = new[] { "US", "BR" },
        Title = "Document for Publication > Best Report Yet!!",
        PublishDate = "2024-08-26T18:19:59Z",
        Status = 3,
        TestRun = true
    };

    [Fact]
    public async Task JsonToXml_MultipleContacts_ReturnsFileAndValidXml()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var res = await client.PostAsJsonAsync("/convert/json-to-xml", BuildPayload());

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("application/xml");

        var fileName = res.Content.Headers.ContentDisposition!.FileName ?? res.Content.Headers.ContentDisposition!.FileNameStar;
        fileName.Should().NotBeNull();
        fileName!.Trim('"').Should().EndWith(".xml");

        var xml = await res.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(xml);

        doc.Root!.Name.LocalName.Should().Be("PublishedItem");
        doc.Root!.Element("Title")!.Value.Should().Be("Document for Publication > Best Report Yet!!");
        doc.Root!.Element("Countries")!.Value.Should().Be("US,BR");
        doc.Root!.Element("PublishedDate")!.Value.Should().Be("2024-08-26T18:19:59Z");
    }

    [Fact]
    public async Task JsonToXml_MultipleContacts_BuildsThreeGroupsWithSequentialSequencesAndHeaders()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var res = await client.PostAsJsonAsync("/convert/json-to-xml", BuildPayload());
        var xml = await res.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(xml);

        // Assert
        var groups = doc.Root!
            .Element("ContactInformation")!
            .Elements("PersonGroup")
            .ToList();

        groups.Count.Should().Be(3);

        groups[0].Attribute("sequence")!.Value.Should().Be("1");
        groups[1].Attribute("sequence")!.Value.Should().Be("2");
        groups[2].Attribute("sequence")!.Value.Should().Be("3");

        groups[0].Element("Name")!.Value.Should().Be("Media Contact");
        groups[1].Element("Name")!.Value.Should().Be("Analytical Contacts");
        groups[2].Element("Name")!.Value.Should().Be("Analytical Contacts");
    }

    [Fact]
    public async Task JsonToXml_MultipleContacts_MapsFirstGroupMikeCorrectlyAndNormalizesPhone()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var res = await client.PostAsJsonAsync("/convert/json-to-xml", BuildPayload());
        var xml = await res.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(xml);

        // Assert
        var firstMemberPerson = doc.Root!
            .Element("ContactInformation")!
            .Elements("PersonGroup").ElementAt(0)!
            .Elements("PersonGroupMember").Single()!
            .Element("Person")!;

        firstMemberPerson.Element("GivenName")!.Value.Should().Be("Mike");
        firstMemberPerson.Element("FamilyName")!.Value.Should().Be("Johnsen");
        firstMemberPerson.Element("DisplayName")!.Value.Should().Be("Mike Johnsen");
        firstMemberPerson.Element("JobTitle")!.Value.Should().Be("Director of Communications & Marketing");

        var number = firstMemberPerson
            .Element("ContactInfo")!
            .Element("Phone")!
            .Element("Number")!.Value;

        number.Should().Be("1-646-731-1347");
    }

    [Fact]
    public async Task JsonToXml_MultipleContacts_MapsSecondGroupFabioAndThirdGroupFabio2()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var res = await client.PostAsJsonAsync("/convert/json-to-xml", BuildPayload());
        var xml = await res.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(xml);

        // Assert
        var groups = doc.Root!
            .Element("ContactInformation")!
            .Elements("PersonGroup")
            .ToList();

        var g2person = groups[1]
            .Elements("PersonGroupMember").Single()
            .Element("Person")!;

        g2person.Element("GivenName")!.Value.Should().Be("Fabio");
        g2person.Element("FamilyName")!.Value.Should().Be("Camillo");
        g2person.Element("DisplayName")!.Value.Should().Be("Fabio Camillo");
        g2person.Element("JobTitle")!.Value.Should().Be("CTO");
        g2person.Element("ContactInfo")!.Element("Phone")!.Element("Number")!.Value
            .Should().Be("1-777-731-1347");

        var g3person = groups[2]
            .Elements("PersonGroupMember").Single()
            .Element("Person")!;

        g3person.Element("GivenName")!.Value.Should().Be("Fabio 2");
        g3person.Element("FamilyName")!.Value.Should().Be("Camillo 2");
        g3person.Element("DisplayName")!.Value.Should().Be("Fabio 2 Camillo 2");
        g3person.Element("JobTitle")!.Value.Should().Be("CTO2");

        var g3number = g3person.Element("ContactInfo")!.Element("Phone")!.Element("Number")!.Value;
        g3number.Should().Be("1-777-888-1347");
    }

    [Fact]
    public async Task JsonToXml_MultipleContacts_SkipsBlankContacts()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var res = await client.PostAsJsonAsync("/convert/json-to-xml", BuildPayload());
        var xml = await res.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(xml);

        // Assert
        var groups = doc.Root!
            .Element("ContactInformation")!
            .Elements("PersonGroup")
            .ToList();

        groups.Count.Should().Be(3);

        foreach (var g in groups)
        {
            var p = g.Element("PersonGroupMember")!.Element("Person")!;
            p.Element("GivenName")!.Value.Should().NotBeNullOrEmpty();
            p.Element("FamilyName")!.Value.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task JsonToXml_InvalidPublishDate_ReturnsProblemDetails()
    {
        // Arrange
        var client = _factory.CreateClient();
        var req = new { Status = 3, PublishDate = "2024-08-23T23:59:59Z", TestRun = true }; // too early

        // Act
        var res = await client.PostAsJsonAsync("/convert/json-to-xml", req);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        res.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var json = await res.Content.ReadAsStringAsync();
        json.Should().Contain("\"title\":\"One or more validation errors occurred.\"");
        json.Should().Contain("PublishDate must be on or after 2024-08-24.");
    }
}
