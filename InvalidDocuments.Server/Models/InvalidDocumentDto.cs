namespace InvalidDocuments.Server.Models;

public class InvalidDocumentDto
{
    public string Number { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Series { get; set; } = string.Empty;

    public bool IsRegistered { get; set; }

    public string RegisteredFrom { get; set; } = string.Empty;

    public bool BadRequest { get; set; }

    public string Error { get; set; } = string.Empty;
}