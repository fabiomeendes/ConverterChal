using System.Xml.Serialization;

namespace Converter.Api.Contracts;

[XmlRoot("PublishedItem")]
public class PublishedItemXml
{
    [XmlElement(Order = 1)]
    public string? Title { get; set; }

    [XmlElement(Order = 2)]
    public string? Countries { get; set; }

    [XmlElement("PublishedDate", Order = 3)]
    public string? PublishedDate { get; set; }

    [XmlElement(Order = 4)]
    public ContactInformationXml? ContactInformation { get; set; }
}

public class ContactInformationXml
{
    // ALLOW MULTIPLE GROUPS
    [XmlElement("PersonGroup", Order = 1)]
    public List<PersonGroupXml> PersonGroups { get; set; } = new();
}

public class PersonGroupXml
{
    [XmlAttribute("sequence")]
    public int Sequence { get; set; }

    [XmlElement(Order = 1)]
    public string? Name { get; set; }

    [XmlElement("PersonGroupMember", Order = 2)]
    public List<PersonGroupMemberXml> Members { get; set; } = new();
}

public class PersonGroupMemberXml
{
    [XmlElement("Person", Order = 1)]
    public PersonXml? Person { get; set; }
}

public class PersonXml
{
    [XmlElement(Order = 1)]
    public string? FamilyName { get; set; }

    [XmlElement(Order = 2)]
    public string? GivenName { get; set; }

    [XmlElement(Order = 3)]
    public string? DisplayName { get; set; }

    [XmlElement("JobTitle", Order = 4)]
    public string? JobTitle { get; set; }

    [XmlElement(Order = 6)]
    public ContactInfoXml? ContactInfo { get; set; }
}

public class ContactInfoXml
{
    [XmlElement("Phone", Order = 1)]
    public PhoneXml? Phone { get; set; }
}

public class PhoneXml
{
    [XmlElement("Number", Order = 1)]
    public string? Number { get; set; }
}
