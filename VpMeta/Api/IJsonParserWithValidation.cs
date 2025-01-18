using System.Text.Json;
using System.Text.Json.Nodes;

namespace VpMeta.Api;

public class ApiValidationException(IEnumerable<string> errors) : Exception {
    public IEnumerable<string> Errors { get; private set; } = errors;

}
public interface IJsonParserWithValidation
{
    /// <exception cref="JsonException"/>
    /// <exception cref="ApiValidationException"/>
    JsonNode Parse(string content);
}
