using System.Xml.Serialization;

namespace InvalidDocuments.Server.Models;

[XmlRoot(ElementName = "doklady_neplatne")]
public class InvalidDocument
{
    [XmlElement(ElementName = "dotaz")]
    public Request Request { get; set; } = new();

    [XmlElement(ElementName = "odpoved")]
    public Response Response { get; set; } = new();

    [XmlElement(ElementName = "chyba")]
    public Error Error { get; set; } = new();

    [XmlAttribute(AttributeName = "posl_zmena")]
    public string LastChange { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "pristi_zmeny")]
    public string NextChanges { get; set; } = string.Empty;
}

public class Request
{
    [XmlAttribute(AttributeName = "typ")]
    public string Type { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "cislo")]
    public string Number { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "serie")]
    public string Series { get; set; } = string.Empty;
}

public class Response
{
    [XmlAttribute(AttributeName = "aktualizovano")]
    public string Updated { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "evidovano")]
    public string Registered { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "evidovano_od")]
    public string RegisteredFrom { get; set; } = string.Empty;
}

public class Error
{
    [XmlAttribute(AttributeName = "spatny_dotaz")]
    public string BadRequest { get; set; } = string.Empty;        

    [XmlText]
    public string Text { get; set; } = string.Empty;
}