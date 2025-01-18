using VpMeta.Api;

namespace VpMeta.Tests.Unit;

public class JsonValidationTest
{
    public JsonValidationTest() {
        var stream = typeof(NJsonParser).Assembly.GetManifestResourceStream("VpMeta.schema.json") ?? throw new Exception("Missing schema resource");
        parser = new NJsonParser(stream);
    }
    readonly IJsonParserWithValidation parser;
    
    [Fact]
    public void ThrowsOnNotJson()
    {
        Assert.ThrowsAny<Exception>(() => parser.Parse(""));
        Assert.ThrowsAny<Exception>(() => parser.Parse("asd"));
    }

    [Fact]
    public void ValidJsonProducesNoErrors()
    {
        var json = """
        {
            "trialId": "test",
            "title": "test trial",
            "status": "Not Started",
            "startDate": "2025-01-16"
        }
        """;
        Assert.NotNull(parser.Parse(json));
        json = """
        {
            "trialId": "test",
            "title": "test trial",
            "status": "Not Started",
            "startDate": "2025-01-16",
            "endDate": "2025-02-16",
            "participants": 1
        }
        """;
        Assert.NotNull(parser.Parse(json));
    }

    [Fact]
    public void MissingRequiredFields(){
        var json = """
        {
            "trialId": "test",
            "title": "test trial",
        }
        """;
        Assert.Throws<ApiValidationException>(() => parser.Parse(json));
    }

    [Fact]
    public void ExcessFields(){
        var json = """
        {
            "trialId": "test",
            "title": "test trial",
            "status": "Not Started",
            "startDate": "2025-01-16",
            "aaa": 1
        }
        """;
        Assert.Throws<ApiValidationException>(() => parser.Parse(json));
    }


    [Fact]
    public void InvalidNumber(){
        var json = """
        {
            "trialId": "test",
            "title": "test trial",
            "status": "Not Started",
            "startDate": "2025-01-16",
            "participants": 1.5
        }
        """;
        Assert.Throws<ApiValidationException>(() => parser.Parse(json));
        json = """
        {
            "trialId": "test",
            "title": "test trial",
            "status": "Not Started",
            "startDate": "2025-01-16",
            "participants": 0
        }
        """;
        Assert.Throws<ApiValidationException>(() => parser.Parse(json));
    }
}