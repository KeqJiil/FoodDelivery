using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Api.OpenApi;

/// <summary>
/// If a DTO exposes a public static "Example" property of its own type, serializes it and sets
/// it as the schema's OpenAPI example, so Swagger UI's "Try it out" starts from realistic,
/// hand-picked values instead of an auto-generated placeholder (which for types like TimeOnly is
/// actively wrong — see TimeOnlyExampleTransformer's history for why that matters here).
/// </summary>
public sealed class ExampleValuesSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        var exampleProperty = type.GetProperty("Example", BindingFlags.Public | BindingFlags.Static);
        if (exampleProperty is null || exampleProperty.PropertyType != type) return Task.CompletedTask;

        var example = exampleProperty.GetValue(null);
        if (example is null) return Task.CompletedTask;

        schema.Example = JsonSerializer.SerializeToNode(example, context.JsonTypeInfo);

        return Task.CompletedTask;
    }
}
