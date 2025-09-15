using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Converter.Api.Contracts;
using Converter.Core.Converters;
using Converter.Core.Contracts;

namespace Converter.Api.Services;

public sealed class JsonToPublishedItemXmlConverter : IPublishedItemXmlConverter
{
    public Task<string> ConvertAsync(InputDto input)
    {

        var model = MapToXmlModel(input);

        var serializer = new XmlSerializer(typeof(PublishedItemXml));
        var ns = new XmlSerializerNamespaces();
        ns.Add(string.Empty, string.Empty);

        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            OmitXmlDeclaration = false
        };

        using var ms = new MemoryStream();
        using (var writer = XmlWriter.Create(ms, settings))
        {
            serializer.Serialize(writer, model, ns);
        }

        return Task.FromResult(Encoding.UTF8.GetString(ms.ToArray()));
    }

    private static PublishedItemXml MapToXmlModel(InputDto dto)
    {
        var title = dto.Title ?? dto.ReportMetadata?.Title ?? string.Empty;
        var countries = (dto.CountryIds is { Count: > 0 })
            ? string.Join(",", dto.CountryIds)
            : string.Empty;
        var publishedDate = dto.PublishDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

        return new PublishedItemXml
        {
            Title = title,
            Countries = countries,
            PublishedDate = publishedDate,
            ContactInformation = BuildContactInformation(dto)
        };
    }

    // ONE PersonGroup PER CONTACT, Name = ContactHeader, sequence++ per group
    private static ContactInformationXml? BuildContactInformation(InputDto dto)
    {
        var sections = dto.ReportMetadata?.ContactSection;
        if (sections == null || sections.Count == 0) return null;

        var result = new ContactInformationXml();
        int sequence = 1;

        foreach (var section in sections)
        {
            var infos = section.ContactInformation;
            if (infos == null) continue;

            foreach (var info in infos)
            {
                var groupName = string.IsNullOrWhiteSpace(info.ContactHeader)
                    ? "Contacts"
                    : info.ContactHeader!.Trim();

                var contacts = info.Contacts ?? new List<ContactDto>();
                foreach (var c in contacts)
                {
                    // skip blank
                    if (string.IsNullOrWhiteSpace(c.FirstName) && string.IsNullOrWhiteSpace(c.LastName))
                        continue;

                    var group = new PersonGroupXml
                    {
                        Sequence = sequence++,
                        Name = groupName,
                        Members =
                        {
                            new PersonGroupMemberXml
                            {
                                Person = new PersonXml
                                {
                                    FamilyName = c.LastName,
                                    GivenName = c.FirstName,
                                    DisplayName = BuildDisplayName(c.FirstName, c.LastName),
                                    JobTitle = c.Title,
                                    ContactInfo = BuildContactInfo(c)
                                }
                            }
                        }
                    };

                    result.PersonGroups.Add(group);
                }
            }
        }

        return result.PersonGroups.Count == 0 ? null : result;
    }

    private static ContactInfoXml? BuildContactInfo(ContactDto c)
    {
        if (string.IsNullOrWhiteSpace(c.PhoneNumber)) return null;
        return new ContactInfoXml
        {
            Phone = new PhoneXml { Number = NormalizePhone(c.PhoneNumber!) }
        };
    }

    private static string BuildDisplayName(string? first, string? last)
    {
        var f = (first ?? string.Empty).Trim();
        var l = (last ?? string.Empty).Trim();
        if (f.Length == 0) return l;
        if (l.Length == 0) return f;
        return $"{f} {l}";
    }

    // "+1 646-731-1347" -> "1-646-731-1347"
    private static string NormalizePhone(string raw)
    {
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.Length == 11 && digits.StartsWith("1"))
            return $"{digits[0]}-{digits.Substring(1, 3)}-{digits.Substring(4, 3)}-{digits.Substring(7, 4)}";

        return raw;
    }
}