using InvalidDocuments.Server.Enums;
using InvalidDocuments.Server.Models;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace InvalidDocuments.Server.Helpers;

/// <summary>
/// Provides helper methods for various operations within the Invalid Documents application.
/// </summary>
public static class Helper
{
    /// <summary>
    /// Validates the format and length of a document number.
    /// </summary>
    /// <param name="number">The document number to validate.</param>
    /// <param name="error">The error message if validation fails.</param>
    /// <returns>True if the document number is valid; otherwise, false.</returns>
    public static bool IsValidDocumentNumber(string? number, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrEmpty(number))
        {
            error = "Číslo dokladu nemůže být prázdné.";
            return false;
        }

        if (number.Length > 10)
        {
            error = "Číslo dokladu může obsahovat max. 10 znaků. ";
        }

        string pattern = @"^[a-zA-Z0-9]*$";
        bool isValid = Regex.IsMatch(number, pattern);

        if (!isValid)
        {
            error += "Číslo dokladu obsahuje neplatné znaky.";
        }

        return error == string.Empty;
    }

    /// <summary>
    /// Removes white spaces from a string.
    /// </summary>
    /// <param name="text">The input string.</param>
    /// <returns>The input string without white spaces.</returns>
    public static string RemoveWhiteSpace(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return Regex.Replace(text, @"\s+", "");
    }

    /// <summary>
    /// Deserializes XML string to the specified object type.
    /// </summary>
    /// <typeparam name="TObject">The type of the object to deserialize to.</typeparam>
    /// <param name="xml">The XML string to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public static TObject DeserializeXmlToObject<TObject>(string xml) where TObject : class
    {
        ArgumentNullException.ThrowIfNull(xml);

        ILogger logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(nameof(DeserializeXmlToObject));

        try
        {
            var serializer = new XmlSerializer(typeof(TObject));
            using var reader = new StringReader(xml);

            if (serializer.Deserialize(reader) is TObject result)
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException($"Nepodařilo se deserializovat XML do {typeof(TObject)}.");
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

    /// <summary>
    /// Converts an InvalidDocument object to an InvalidDocumentDto object.
    /// </summary>
    /// <param name="document">The InvalidDocument object to convert.</param>
    /// <returns>The corresponding InvalidDocumentDto object.</returns>
    public static InvalidDocumentDto GetInvalidDocumentDto(this InvalidDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new InvalidDocumentDto
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
    internal static string GetDocumentType(string type)
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