using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Api.OpenApi;

/// <summary>
/// Pulls each DTO's XML &lt;summary&gt; into the schema description, and each property's doc
/// comment into that property's schema description. For positional records (every request DTO
/// in this API), the property doc lives as a &lt;param&gt; on the record's own &lt;summary&gt;
/// block rather than on a synthesized property, so that's checked as a fallback.
/// </summary>
public sealed class XmlCommentsSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        var docs = XmlDocumentation.For(type.Assembly);

        if (string.IsNullOrEmpty(schema.Description))
        {
            var typeSummary = docs.SummaryFor(type);
            if (!string.IsNullOrEmpty(typeSummary))
                schema.Description = typeSummary;
        }

        if (schema.Properties is null) return Task.CompletedTask;

        foreach (var jsonProperty in context.JsonTypeInfo.Properties)
        {
            if (!schema.Properties.TryGetValue(jsonProperty.Name, out var propertySchema) ||
                propertySchema is not OpenApiSchema concretePropertySchema)
                continue;

            var propertyInfo = jsonProperty.AttributeProvider as PropertyInfo;

            var description = propertyInfo is not null ? docs.SummaryFor(propertyInfo) : null;
            description ??= docs.ParamSummary(type, propertyInfo?.Name ?? jsonProperty.Name);

            if (!string.IsNullOrEmpty(description))
                concretePropertySchema.Description = description;
        }

        return Task.CompletedTask;
    }
}
