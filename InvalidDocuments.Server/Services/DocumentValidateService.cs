using System.Xml.Serialization;

namespace InvalidDocuments.Server.Services;

/// <summary>
/// Provides document validation services. 
/// This service implements the <seealso cref="IDocumentValidateService"/> interface.
/// </summary>
/// <param name="httpClient">The HttpClient instance for sending HTTP requests and receiving HTTP responses.</param>
/// <param name="configuration">The IConfiguration instance for accessing application configuration.</param>
/// <param name="logger">The ILogger instance for logging errors and information.</param>
internal sealed class DocumentValidateService(HttpClient httpClient, IConfiguration configuration, ILogger<DocumentValidateService> logger) : IDocumentValidateService
{
    public async Task<DocumentValidationResult> ValidateDocumentAsync(string documentNumber, CancellationToken cancellationToken = default)
    {
        string errorMessage = string.Empty;

        if (string.IsNullOrEmpty(documentNumber))
        {
            return new DocumentValidationResult
            {
                Error = "Číslo dokladu nemůže být prázdné."
            };
        }

        try
        {
            var documentTypes = configuration.GetSection("DocumentTypes").Get<List<int>>();
            ArgumentNullException.ThrowIfNull(documentTypes);

            var tasks = new List<Task<DocumentValidationResult>>();

            documentTypes.ForEach(documentType => tasks.Add(ValidateDocumentByDocumentTypeAsync(documentNumber, documentType, cancellationToken)));

            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);

                if (completedTask.Result.IsRegistered)
                {
                    return await completedTask;
                }
            }
        }
        catch (ArgumentNullException ex)
        {
            logger.LogError(ex.ToString());
            errorMessage = "Chyba při zpracování požadavku. O problému víme a pracujeme na nápravě.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            errorMessage = "Neočekávaná chyba. O problému víme a pracujeme na nápravě.";
        }

        return new DocumentValidationResult
        {
            Number = documentNumber,
            Error = errorMessage
        };
    }

    internal async Task<DocumentValidationResult> ValidateDocumentByDocumentTypeAsync(string number, int type, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(number);

        var baseUrlApi = configuration["BaseUrlApiMvcr"];
        ArgumentException.ThrowIfNullOrEmpty(baseUrlApi);

        try
        {
            var requestUri = $"{baseUrlApi}?dotaz={number}&doklad={type}";

            var data = await httpClient.GetStringAsync(requestUri, cancellationToken);
            var invalidDocument = DeserializeXmlToInvalidDocument(data) ?? new();

            return MapInvalidDocumentToDocumentValidationResult(invalidDocument);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            throw;
        }
    }

    internal InvalidDocument DeserializeXmlToInvalidDocument(string xml)
    {
        ArgumentNullException.ThrowIfNull(xml);

        try
        {
            var serializer = new XmlSerializer(typeof(InvalidDocument));
            using var reader = new StringReader(xml);

            if (serializer.Deserialize(reader) is InvalidDocument result)
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException($"Nepodařilo se deserializovat XML do {typeof(InvalidDocument)}.");
            }
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex.ToString());
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Neočekávaná chyba při deserializaci XML na objekt.");
            throw;
        }
    }

    internal DocumentValidationResult MapInvalidDocumentToDocumentValidationResult(InvalidDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new DocumentValidationResult
        {
            Number = document.Request.Number,
            Series = document.Request.Series != "-" ? document.Request.Series : string.Empty,
            Type = GetDocumentType(document.Request.Type),
            IsRegistered = document.Response.Registered == TruthValue.ano.ToString(),
            RegisteredFrom = document.Response.RegisteredFrom,
            BadRequest = document.Error.BadRequest == TruthValue.ano.ToString(),
            Error = document.Error.Text
        };
    }

    /// <summary>
    /// Gets the human-readable document type based on the provided document type code.
    /// </summary>
    /// <param name="type">The document type code.</param>
    /// <returns>The human-readable document type.</returns>
    internal string GetDocumentType(string type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type switch
        {
            "OP" or "OPs" => "občanský průkaz",
            "CD" or "CDj" => "cestovní pas",
            "ZP" => "zbrojní průkaz / zbrojní licence",
            _ => string.Empty,
        };
    }
}

/// <summary>
/// Represents a data model for deserializing XML.
/// </summary>
[XmlRoot(ElementName = "doklady_neplatne")]
public sealed class InvalidDocument
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
public sealed class Request
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
public sealed class Response
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
public sealed class Error
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

/// <summary>
/// Represents the result of document validation.
/// </summary>
public sealed record DocumentValidationResult
{
    /// <summary>
    /// Gets or sets the document number.
    /// </summary>
    public string Number { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the document series.
    /// </summary>
    public string Series { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the document is registered.
    /// </summary>
    public bool IsRegistered { get; init; }

    /// <summary>
    /// Gets or sets the date when the document was registered.
    /// </summary>
    public string RegisteredFrom { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the request was bad.
    /// </summary>
    public bool BadRequest { get; init; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Error { get; init; } = string.Empty;
}

/// <summary>
/// Represents a truth value enumeration in Czech.
/// </summary>
public enum TruthValue
{
    /// <summary>
    /// Represents the "yes" truth value.
    /// </summary>
    ano,

    /// <summary>
    /// Represents the "no" truth value.
    /// </summary>
    ne
}