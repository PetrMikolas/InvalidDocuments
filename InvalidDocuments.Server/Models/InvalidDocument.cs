using System.Xml.Serialization;

namespace InvalidDocuments.Server.Models;

/// <summary>
/// Represents a data model for an invalid document in the Invalid Documents application.
/// </summary>
[XmlRoot(ElementName = "doklady_neplatne")]
public class InvalidDocument
{
    /// <summary>
    /// Gets or sets the request details.
    /// </summary>
    [XmlElement(ElementName = "dotaz")]
    public Request Request { get; set; } = new();

    /// <summary>
    /// Gets or sets the response details.
    /// </summary>
    [XmlElement(ElementName = "odpoved")]
    public Response Response { get; set; } = new();

    /// <summary>
    /// Gets or sets the error details.
    /// </summary>
    [XmlElement(ElementName = "chyba")]
    public Error Error { get; set; } = new();

    /// <summary>
    /// Gets or sets the date of the last change.
    /// </summary>
    [XmlAttribute(AttributeName = "posl_zmena")]
    public string LastChange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date of the next changes.
    /// </summary>
    [XmlAttribute(AttributeName = "pristi_zmeny")]
    public string NextChanges { get; set; } = string.Empty;
}

/// <summary>
/// Represents the request details.
/// </summary>
public class Request
{
    /// <summary>
    /// Gets or sets the type of the document.
    /// </summary>
    [XmlAttribute(AttributeName = "typ")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of the document.
    /// </summary>
    [XmlAttribute(AttributeName = "cislo")]
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the series of the document.
    /// </summary>
    [XmlAttribute(AttributeName = "serie")]
    public string Series { get; set; } = string.Empty;
}

/// <summary>
/// Represents the response details.
/// </summary>
public class Response
{
    /// <summary>
    /// Gets or sets the date of the last update.
    /// </summary>
    [XmlAttribute(AttributeName = "aktualizovano")]
    public string Updated { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the document is registered.
    /// </summary>
    [XmlAttribute(AttributeName = "evidovano")]
    public string Registered { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the document was registered.
    /// </summary>
    [XmlAttribute(AttributeName = "evidovano_od")]
    public string RegisteredFrom { get; set; } = string.Empty;
}

/// <summary>
/// Represents the error details.
/// </summary>
public class Error
{
    /// <summary>
    /// Gets or sets whether the request is bad.
    /// </summary>
    [XmlAttribute(AttributeName = "spatny_dotaz")]
    public string BadRequest { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error text.
    /// </summary>
    [XmlText]
    public string Text { get; set; } = string.Empty;
}
