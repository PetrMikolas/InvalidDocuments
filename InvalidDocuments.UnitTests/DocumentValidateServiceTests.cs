using FluentAssertions;
using InvalidDocuments.Server.Endpoints;
using InvalidDocuments.Server.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework.Internal;

namespace InvalidDocuments.UnitTests;

internal class DocumentValidateServiceTests
{
    private InvalidDocument _invalidDocument;
    private DocumentValidateService _documentValidateService;

    [SetUp]
    public void Setup()
    {
        ILogger<DocumentValidateService> logger = new NullLogger<DocumentValidateService>();
        _documentValidateService = new DocumentValidateService(null!, null!, logger);

        _invalidDocument = new InvalidDocument
        {
            Request = new() { Number = "183579", Series = "AA81", Type = "OPs" },
            Response = new() { Registered = "ano", RegisteredFrom = "15.4.2024", Updated = "24.4.2024" },
            Error = new(),
            LastChange = "12.8.2010",
            NextChanges = string.Empty,
        };
    }

    [Test]
    [TestCase("123456ABCD", true, "")]
    [TestCase("", false, "Číslo dokladu nemůže být prázdné.")]
    [TestCase(null!, false, "Číslo dokladu nemůže být prázdné.")]
    [TestCase("123456789ABCD", false, "Číslo dokladu může obsahovat max. 10 znaků. ")]
    [TestCase("1234*56", false, "Číslo dokladu obsahuje neplatné znaky.")]
    public void IsValidDocumentNumber_Success(string number, bool expectedResult, string expectedError)
    {
        // Act
        var isValid = EndpointsValidationsRegistrationExtensions.IsValidDocumentNumber(number, out string error);

        // Assert     
        isValid.Should().Be(expectedResult);
        error.Should().Be(expectedError);
    }

    [Test]
    public void RemoveWhiteSpace_Success()
    {
        // Arrange 
        string text = " 1 2 3 4 A B C D ";

        // Act
        var cleanedText = EndpointsValidationsRegistrationExtensions.RemoveWhiteSpace(text);

        // Assert     
        cleanedText.Should().Be("1234ABCD");
    }

    [Test]
    public void RemoveWhiteSpace_TextIsNull_ArgumentNullException()
    {
        // Act 
        Action act = () => EndpointsValidationsRegistrationExtensions.RemoveWhiteSpace(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void DeserializeXmlToInvalidDocument_Success()
    {
        // Arrange 
        string xml = "<doklady_neplatne posl_zmena=\"12.8.2010\" pristi_zmeny=\"\">" +
                        "<dotaz typ=\"OPs\" cislo=\"183579\" serie=\"AA81\"/>" +
                        "<odpoved aktualizovano=\"24.4.2024\" evidovano=\"ano\" evidovano_od=\"15.4.2024\"/>" +
                     "</doklady_neplatne>";

        // Act
        var deserializedDocument = _documentValidateService.DeserializeXmlToInvalidDocument(xml);

        // Assert     
        deserializedDocument.Should().BeEquivalentTo(_invalidDocument);
    }

    [Test]
    public void DeserializeXmlToInvalidDocument_XmlIsNull_ArgumentNullException()
    {
        // Act 
        Action act = () => _documentValidateService.DeserializeXmlToInvalidDocument(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void DeserializeXmlToInvalidDocument_XmlIsInvalid_InvalidOperationException()
    {
        // Arrange 
        string xml = "<invalid_xml></invalid_xml>";

        // Act 
        Action act = () => _documentValidateService.DeserializeXmlToInvalidDocument(xml);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void MapInvalidDocumentToDocumentValidationResult_Success()
    {
        // Arrange  
        var expectedDto = new DocumentValidationResult
        {
            Number = "183579",
            Series = "AA81",
            Type = "občanský průkaz",
            IsRegistered = true,
            RegisteredFrom = "15.4.2024",
            BadRequest = false,
            Error = string.Empty,
        };

        // Act
        var dto = _documentValidateService.MapInvalidDocumentToDocumentValidationResult(_invalidDocument);

        // Assert     
        dto.Should().BeEquivalentTo(expectedDto);
    }

    [Test]
    public void MapInvalidDocumentToDocumentValidationResult_InvalidDocumentIsNull_ArgumentNullException()
    {
        // Act 
        Action act = () => _documentValidateService.MapInvalidDocumentToDocumentValidationResult(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    [TestCase("OP", "občanský průkaz")]
    [TestCase("OPs", "občanský průkaz")]
    [TestCase("CD", "cestovní pas")]
    [TestCase("CDj", "cestovní pas")]
    [TestCase("ZP", "zbrojní průkaz / zbrojní licence")]
    [TestCase("", "")]
    public void GetDocumentType_Success(string input, string expectedOutput)
    {
        // Act
        var type = _documentValidateService.GetDocumentType(input);

        // Assert     
        type.Should().Be(expectedOutput);
    }

    [Test]
    public void GetDocumentType_TypeIsNull_ArgumentNullException()
    {
        // Act 
        Action act = () => _documentValidateService.GetDocumentType(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}