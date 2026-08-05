using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Api.OpenApi;

/// <summary>
/// Pulls each controller action's XML &lt;summary&gt; into the operation summary, and each
/// action parameter's &lt;param&gt; into that parameter's description, so Swagger UI shows the
/// same explanation a developer reading the controller source would see.
/// </summary>
public sealed class XmlCommentsOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor controllerAction)
            return Task.CompletedTask;

        var method = controllerAction.MethodInfo;
        var docs = XmlDocumentation.For(method.DeclaringType!.Assembly);

        var summary = docs.SummaryFor(method);
        if (!string.IsNullOrEmpty(summary))
            operation.Summary = summary;

        if (operation.Parameters is null) return Task.CompletedTask;

        var complexParameterTypes = method.GetParameters()
            .Where(p => p.ParameterType is { IsPrimitive: false, IsEnum: false } && p.ParameterType != typeof(string) &&
                        p.ParameterType != typeof(Guid) && p.ParameterType != typeof(CancellationToken))
            .Select(p => p.ParameterType)
            .ToList();

        foreach (var parameter in operation.Parameters)
        {
            // Method-level <param> covers simple route/body parameters (e.g. "id").
            var description = docs.ParamSummary(method, parameter.Name!);

            // [FromQuery]/[FromRoute] complex types get expanded into one OpenAPI parameter per
            // property; their descriptions live as <param> on the DTO's own type-level summary.
            description ??= complexParameterTypes
                .Select(t => docs.ParamSummary(t, parameter.Name!))
                .FirstOrDefault(d => d is not null);

            if (!string.IsNullOrEmpty(description))
                parameter.Description = description;
        }

        return Task.CompletedTask;
    }
}
