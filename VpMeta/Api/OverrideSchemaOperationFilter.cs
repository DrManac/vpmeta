using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NJsonSchema;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VpMeta.Api;

[AttributeUsage(AttributeTargets.Method)]
public class OverrideSchemaAttribute : Attribute {}

public class OverrideSchemaOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var overrideAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<OverrideSchemaAttribute>() ?? [];

        if (overrideAttributes.Any())
        {
            // Load the JSON schema file and convert it to OpenApiSchema
            var jsonSchema = LoadJsonSchema("VpMeta.schema.json");

            operation.RequestBody = new OpenApiRequestBody
            {
                
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = jsonSchema
                    }
                }
            };
        }
    }

    private static OpenApiSchema LoadJsonSchema(string resourcePath)
    {
        var json = typeof(Program).Assembly.GetManifestResourceStream(resourcePath) ?? throw new Exception("Missing schema resource");
        var schema = JsonSchema.FromJsonAsync(json).Result;

        var openApiSchema = new OpenApiSchema
        {
            Type = ConvertJsonObjectTypeToOpenApiType(schema.Type),
            Properties = schema.Properties.ToDictionary(
                prop => prop.Key,
                prop => new OpenApiSchema
                {
                    Type = ConvertJsonObjectTypeToOpenApiType(prop.Value.Type),
                    Format = prop.Value.Format,
                    Minimum = prop.Value.Minimum,
                    Enum = [.. prop.Value.Enumeration.Select(x => new OpenApiString(x as string) as IOpenApiAny)]
                }),
            Required = schema.RequiredProperties.ToHashSet()
            
        };

        return openApiSchema;
    }

    private static string ConvertJsonObjectTypeToOpenApiType(JsonObjectType jsonObjectType)
    {
        return jsonObjectType switch
        {
            JsonObjectType.String => "string",
            JsonObjectType.Number => "number",
            JsonObjectType.Integer => "integer",
            JsonObjectType.Boolean => "boolean",
            JsonObjectType.Array => "array",
            JsonObjectType.Object => "object",
            _ => "string"
        };
    }

}
