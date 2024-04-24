using InvalidDocuments.Server.Enums;
using InvalidDocuments.Server.Models;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace InvalidDocuments.Server.Helpers;

public static class Helper
{
    public static bool IsValidDocumentNumber(string? number, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrEmpty(number))
        {
            error = $"Číslo dokladu nemůže být prázdné.";
            return false;
        }

        if (number.Length > 10)
        {
            error = $"Číslo dokladu může obsahovat max. 10 znaků. ";
        }

        string pattern = @"^[a-zA-Z0-9]*$";
        bool isValid = Regex.IsMatch(number, pattern);

        if (!isValid)
        {
            error += $"Číslo dokladu obsahuje neplatné znaky.";
        }

        return error == string.Empty;
    }

    public static string RemoveWhiteSpace(this string text)
    {
        return Regex.Replace(text, @"\s+", "");
    }

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

    private static string GetDocumentType(string type)
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