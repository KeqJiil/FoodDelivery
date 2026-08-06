using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;

namespace Api.OpenApi;

/// <summary>
/// Loads a compiled assembly's XML doc-comment file (produced by GenerateDocumentationFile)
/// and exposes lookups keyed by the same member-ID format .NET uses in the file itself
/// ("T:", "M:", "P:" prefixes). Microsoft.AspNetCore.OpenApi has no built-in XML-comments
/// support (unlike Swashbuckle's IncludeXmlComments), so this is the glue that lets
/// &lt;summary&gt;/&lt;param&gt; comments on controllers and DTOs reach the generated schema.
/// </summary>
public sealed class XmlDocumentation
{
    private readonly Dictionary<string, string> _summaries = new();
    private readonly Dictionary<string, Dictionary<string, string>> _params = new();

    private static readonly ConcurrentDictionary<Assembly, XmlDocumentation> Cache = new();

    public static XmlDocumentation For(Assembly assembly) => Cache.GetOrAdd(assembly, Load);

    private static XmlDocumentation Load(Assembly assembly)
    {
        var doc = new XmlDocumentation();
        var xmlPath = Path.ChangeExtension(assembly.Location, ".xml");
        if (!File.Exists(xmlPath)) return doc;

        var xml = XDocument.Load(xmlPath);
        foreach (var member in xml.Descendants("member"))
        {
            var name = member.Attribute("name")?.Value;
            if (name is null) continue;

            var summary = member.Element("summary")?.Value.Trim();
            if (!string.IsNullOrEmpty(summary))
                doc._summaries[name] = CollapseWhitespace(summary);

            var paramDict = member.Elements("param")
                .Where(p => p.Attribute("name") is not null)
                .ToDictionary(p => p.Attribute("name")!.Value, p => CollapseWhitespace(p.Value.Trim()),
                    StringComparer.OrdinalIgnoreCase);
            if (paramDict.Count > 0)
                doc._params[name] = paramDict;
        }

        return doc;
    }

    private static string CollapseWhitespace(string text) =>
        string.Join(' ', text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries));

    public string? SummaryFor(MemberInfo member) => _summaries.GetValueOrDefault(GetMemberId(member));

    public string? SummaryFor(MethodInfo method) => _summaries.GetValueOrDefault(GetMemberId(method));

    public string? ParamSummary(MemberInfo owner, string paramName) =>
        _params.TryGetValue(GetMemberId(owner), out var byName) ? byName.GetValueOrDefault(paramName) : null;

    /// <summary>
    /// Builds the "T:Namespace.Type", "M:Namespace.Type.Method(Params)" or "P:Namespace.Type.Property"
    /// identifiers the C# compiler writes into the XML doc file, per ECMA-334 Annex D.
    /// </summary>
    private static string GetMemberId(MemberInfo member)
    {
        var typeName = (member as Type ?? member.DeclaringType)?.FullName?.Replace('+', '.') ?? "";

        return member switch
        {
            Type => $"T:{typeName}",
            PropertyInfo => $"P:{typeName}.{member.Name}",
            MethodInfo method => $"M:{typeName}.{method.Name}{FormatParameters(method)}",
            _ => $"?:{typeName}.{member.Name}"
        };
    }

    private static string FormatParameters(MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length == 0) return "";

        var formatted = parameters.Select(p => FormatTypeName(p.ParameterType));
        return $"({string.Join(',', formatted)})";
    }

    private static string FormatTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var genericName = type.GetGenericTypeDefinition().FullName!;
            genericName = genericName[..genericName.IndexOf('`')];
            var args = string.Join(',', type.GetGenericArguments().Select(FormatTypeName));
            return $"{genericName}{{{args}}}";
        }

        return type.FullName?.Replace('+', '.') ?? type.Name;
    }
}
