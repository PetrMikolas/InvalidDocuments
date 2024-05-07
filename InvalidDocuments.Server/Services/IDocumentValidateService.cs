namespace InvalidDocuments.Server.Services;

/// <summary>
/// Defines methods for validating document.
/// </summary>
public interface IDocumentValidateService
{
    /// <summary>
    /// Validates the validity of a document against the database of invalid documents of the Ministry of Interior of the Czech Republic (MVČR).
    /// </summary>
    /// <param name="documentNumber">The document number to be validated.</param>
    /// <param name="cancellationToken">The cancellation token. (Optional, defaults to default)</param>
    /// <returns>A DocumentValidationResult object containing the result of the document's validation.</returns>
    Task<DocumentValidationResult> ValidateDocumentAsync(string documentNumber, CancellationToken cancellationToken = default);
}