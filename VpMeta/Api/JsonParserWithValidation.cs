using NJsonSchema;
using System.Text.Json.Nodes;

namespace VpMeta.Api;

public class NJsonParser(Stream schemaStream) : IJsonParserWithValidation
{
    private readonly JsonSchema schema = JsonSchema.FromJsonAsync(schemaStream).Result;
    public JsonNode Parse(string content)
    {
        var errors = schema.Validate(content);
        if (errors.Count > 0) throw new ApiValidationException(errors.Select(x => x.ToString()));
        return JsonNode.Parse(content)!;
    }
}