using System.Text.Json.Serialization;

namespace Converter.Core.Contracts;

public sealed class InputDto
{
    public string? Id { get; set; }
    public ReportMetadataDto? ReportMetadata { get; set; }
    public List<string>? CountryIds { get; set; }
    public string? Title { get; set; }
    public DateTimeOffset PublishDate { get; set; }
    public int Status { get; set; }

    [JsonPropertyName("TestRun")]
    public bool TestRun { get; set; }
}

public sealed class ReportMetadataDto
{
    public string? Title { get; set; }
    public List<ContactSectionDto>? ContactSection { get; set; }
}

public sealed class ContactSectionDto
{
    public List<ContactInformationDto>? ContactInformation { get; set; }
}

public sealed class ContactInformationDto
{
    public string? ContactHeader { get; set; }
    public List<ContactDto>? Contacts { get; set; }
}

public sealed class ContactDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Title { get; set; }
    public string? PhoneNumber { get; set; }
}
