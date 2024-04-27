namespace InvalidDocuments.Server.Models;

/// <summary>
/// Represents a data transfer object (DTO) for an invalid document.
/// </summary>
public sealed class InvalidDocumentDto
{
    /// <summary>
    /// Gets or sets the document number.
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document series.
    /// </summary>
    public string Series { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the document is registered.
    /// </summary>
    public bool IsRegistered { get; set; }

    /// <summary>
    /// Gets or sets the date when the document was registered.
    /// </summary>
    public string RegisteredFrom { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the request was bad.
    /// </summary>
    public bool BadRequest { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Error { get; set; } = string.Empty;
}