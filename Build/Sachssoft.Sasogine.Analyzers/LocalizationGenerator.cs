using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Sachssoft.Sasogine.Analyzers;

/// <summary>Generates strongly typed Sasogine localization keys from XDLT dictionaries.</summary>
[Generator(LanguageNames.CSharp)]
public sealed class LocalizationGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor InvalidXml = Error("SASLOC001", "Invalid localization XML", "{0}");
    private static readonly DiagnosticDescriptor InvalidDictionary = Error("SASLOC002", "Invalid localization dictionary", "{0}");
    private static readonly DiagnosticDescriptor DuplicateKey = Error("SASLOC003", "Duplicate localization key", "Dictionary '{0}' contains duplicate key '{1}'");
    private static readonly DiagnosticDescriptor IdentifierCollision = Error("SASLOC004", "Generated identifier collision", "'{0}' conflicts with another generated member in '{1}'");
    private static readonly DiagnosticDescriptor DuplicateDictionary = Error("SASLOC005", "Duplicate source dictionary", "More than one .xdlt declares dictionary '{0}'");
    private static readonly DiagnosticDescriptor UnknownSource = Error("SASLOC006", "Invalid asset source", "{0}");
    private static readonly DiagnosticDescriptor TranslationMismatch = Error("SASLOC007", "Invalid translation", "{0}");
    private static readonly DiagnosticDescriptor GeneratedTypeCollision = Error("SASLOC008", "Generated type already exists", "Generated type '{0}' conflicts with an existing type");

    /// <summary>Registers incremental localization generation with the Roslyn pipeline.</summary>
    /// <param name="context">The incremental generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var inputs = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".xdlt", StringComparison.OrdinalIgnoreCase) ||
                file.Path.EndsWith(".xdl", StringComparison.OrdinalIgnoreCase))
            .Select(static (file, cancellationToken) => new LocalizationInput(file.Path, file.GetText(cancellationToken)))
            .Collect();

        var options = context.AnalyzerConfigOptionsProvider.Select(static (provider, _) =>
        {
            provider.GlobalOptions.TryGetValue("build_property.RootNamespace", out string? rootNamespace);
            return rootNamespace ?? string.Empty;
        });

        var state = inputs.Combine(context.CompilationProvider).Combine(options);
        context.RegisterSourceOutput(state, static (production, value) =>
            Execute(production, value.Left.Left, value.Left.Right, value.Right));
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<LocalizationInput> inputs,
        Compilation compilation, string rootNamespace)
    {
        var documents = new List<DictionaryDocument>();
        foreach (var input in inputs.OrderBy(static f => f.Path, StringComparer.Ordinal))
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (input.Text is null)
            {
                Report(context, InvalidXml, input, (XElement?)null, "Could not read the localization file.");
                continue;
            }

            DictionaryDocument? document = Parse(context, input);
            if (document is not null) documents.Add(document);
        }

        var templates = new Dictionary<string, DictionaryDocument>(StringComparer.Ordinal);
        var translations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var doc in documents)
        {
            if (doc.IsTemplate)
            {
                if (templates.ContainsKey(doc.Name))
                    Report(context, DuplicateDictionary, doc.Input, doc.Root, doc.Name);
                else templates.Add(doc.Name, doc);
            }
            else if (!translations.Add(doc.Name + "\u0000" + doc.Language))
            {
                Report(context, TranslationMismatch, doc.Input, doc.Root,
                    $"Duplicate translation dictionary '{doc.Name}' for language '{doc.Language}'.");
            }
        }

        foreach (var doc in documents.Where(static x => !x.IsTemplate))
        {
            if (!templates.TryGetValue(doc.Name, out var template))
            {
                Report(context, TranslationMismatch, doc.Input, doc.Root,
                    $"Translation '{doc.Name}' requires a corresponding .xdlt template.");
                continue;
            }

            foreach (var pair in doc.Entries)
            {
                if (!template.Entries.TryGetValue(pair.Key, out var entry))
                    Report(context, TranslationMismatch, doc.Input, pair.Value.Element,
                        $"Key '{pair.Key}' is not declared in dictionary '{doc.Name}.xdlt'.");
                else if (entry.IsAsset != pair.Value.IsAsset)
                    Report(context, TranslationMismatch, doc.Input, pair.Value.Element,
                        $"Key '{pair.Key}' changes between <item> and <asset>.");
            }
        }

        string defaultNamespace = NormalizeNamespace(rootNamespace, compilation.AssemblyName ?? "Sasogine");
        var sourceTypes = templates.Values.Any(static document => document.Entries.Values.Any(static entry => entry.IsAsset))
            ? FindResourceSources(compilation, context.CancellationToken)
            : new List<SourceType>();
        var generatedTypeNames = new HashSet<string>(StringComparer.Ordinal);
        var generatedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var template in templates.Values.OrderBy(static x => x.Name, StringComparer.Ordinal))
        {
            string ns = template.Namespace ?? defaultNamespace;
            string className = NameToIdentifier(template.Name) + "Keys";
            string metadataNamespace = string.Concat(SyntaxFactory.ParseName(ns).DescendantTokens().Select(static token => token.ValueText));
            string metadataName = metadataNamespace + "." + className;
            if (!generatedTypeNames.Add(metadataName) || compilation.GetTypeByMetadataName(metadataName) is not null)
            {
                Report(context, GeneratedTypeCollision, template.Input, template.Root, metadataName);
                continue;
            }

            var tree = new ClassNode(className);
            var keySet = new HashSet<string>(StringComparer.Ordinal);
            bool valid = ReadNodes(context, template, template.Root, tree, keySet, sourceTypes);
            if (!valid) continue;

            var output = new StringBuilder(4096);
            output.AppendLine("// <auto-generated />");
            output.AppendLine("#nullable enable");
            output.Append("namespace ").Append(ns).AppendLine(";").AppendLine();
            AppendClass(output, tree, 0, true);
            string hintName = Path.GetFileName(template.Input.Path) + ".cs";
            if (!generatedFileNames.Add(hintName))
                hintName = className + "/" + hintName;

            context.AddSource(hintName, SourceText.From(output.ToString(), Encoding.UTF8));
        }
    }

    private static DictionaryDocument? Parse(SourceProductionContext context, LocalizationInput input)
    {
        try
        {
            using var reader = XmlReader.Create(new StringReader(input.Text!.ToString()),
                new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
            XDocument xml = XDocument.Load(reader, LoadOptions.SetLineInfo);
            XElement? root = xml.Root;
            if (root is null || root.Name.LocalName != "dictionary")
            {
                Report(context, InvalidDictionary, input, root, "Root element must be <dictionary>.");
                return null;
            }

            string name = ((string?)root.Attribute("name") ?? "").Trim();
            string language = ((string?)root.Attribute("language") ?? "").Trim();
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(language))
            {
                Report(context, InvalidDictionary, input, root, "Dictionary requires nonempty name and language attributes.");
                return null;
            }

            bool isTemplate = input.Path.EndsWith(".xdlt", StringComparison.OrdinalIgnoreCase);
            string? declaredNamespace = isTemplate ? ((string?)root.Attribute("namespace"))?.Trim() : null;
            if (declaredNamespace is not null && !declaredNamespace.Split('.').All(IsNamespaceIdentifier))
            {
                Report(context, InvalidDictionary, input, root,
                    $"The namespace '{declaredNamespace}' must be a dotted C# identifier.");
                return null;
            }

            var doc = new DictionaryDocument(input, root, name, language, isTemplate, declaredNamespace);
            bool valid = CollectEntries(context, doc, root);
            return valid ? doc : null;
        }
        catch (XmlException error)
        {
            Report(context, InvalidXml, input, error.LineNumber, error.Message);
            return null;
        }
        catch (Exception error) when (error is ArgumentException || error is InvalidOperationException)
        {
            Report(context, InvalidXml, input, (int?)null, error.Message);
            return null;
        }
    }

    private static bool CollectEntries(SourceProductionContext context, DictionaryDocument doc, XElement container)
    {
        bool valid = true;
        foreach (var child in container.Elements())
        {
            string kind = child.Name.LocalName;
            if (kind == "section")
            {
                if (string.IsNullOrWhiteSpace((string?)child.Attribute("name")))
                {
                    Report(context, InvalidDictionary, doc.Input, child, "Section requires a name attribute.");
                    valid = false;
                }
                if (!CollectEntries(context, doc, child)) valid = false;
                continue;
            }
            if (kind != "item" && kind != "asset")
            {
                Report(context, InvalidDictionary, doc.Input, child,
                    $"Unsupported element <{kind}>. Expected <section>, <item> or <asset>.");
                valid = false;
                continue;
            }

            string key = ((string?)child.Attribute("name") ?? "").Trim();
            if (key.Length == 0)
            {
                Report(context, InvalidDictionary, doc.Input, child, "Item or asset requires a nonempty name attribute.");
                valid = false;
                continue;
            }
            if (child.HasElements)
            {
                Report(context, InvalidDictionary, doc.Input, child,
                    $"Key '{key}' must contain text, not child XML elements.");
                valid = false;
                continue;
            }
            if (doc.Entries.ContainsKey(key))
            {
                Report(context, DuplicateKey, doc.Input, child, doc.Name, key);
                valid = false;
            }
            else doc.Entries.Add(key, new DictionaryEntry(child, kind == "asset"));
        }
        return valid;
    }

    private static bool ReadNodes(SourceProductionContext context, DictionaryDocument document, XElement container,
        ClassNode node, HashSet<string> allKeys, List<SourceType> sourceTypes)
    {
        bool valid = true;
        var memberNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (XElement child in container.Elements())
        {
            string kind = child.Name.LocalName;
            if (kind == "section")
            {
                string name = NameToIdentifier((string)child.Attribute("name")!);
                if (name == node.Name || !memberNames.Add(name))
                {
                    Report(context, IdentifierCollision, document.Input, child, name, node.Name);
                    valid = false;
                    continue;
                }
                var nested = new ClassNode(name);
                node.Classes.Add(nested);
                if (!ReadNodes(context, document, child, nested, allKeys, sourceTypes)) valid = false;
                continue;
            }
            if (kind != "item" && kind != "asset") continue;

            string key = ((string)child.Attribute("name")!).Trim();
            if (!allKeys.Add(key))
            {
                Report(context, DuplicateKey, document.Input, child, document.Name, key);
                valid = false;
                continue;
            }
            string member = NameToIdentifier(key);
            if (!memberNames.Add(member))
            {
                Report(context, IdentifierCollision, document.Input, child, member, node.Name);
                valid = false;
                continue;
            }
            if (member == node.Name)
            {
                Report(context, IdentifierCollision, document.Input, child, member, node.Name);
                valid = false;
                continue;
            }

            string value = kind == "item" ? child.Value : child.Value.Trim();
            string declaration;
            if (kind == "item")
            {
                declaration = "new(" + CSharpString(key) + ", " + CSharpString(value) + ")";
                node.Fields.Add(new GeneratedField(member, "global::Sachssoft.Engine.Resources.Localization.LocalizedTextEntry", declaration));
            }
            else
            {
                SourceType? source = ResolveResourceSource(context, document, child, sourceTypes);
                if (source is null)
                {
                    valid = false;
                    continue;
                }
                declaration = "new(" + CSharpString(key) + ", new " + source.TypeName + "(" + CSharpString(value) + "))";
                node.Fields.Add(new GeneratedField(member, "global::Sachssoft.Engine.Resources.Localization.LocalizedAssetEntry", declaration));
            }
        }
        return valid;
    }

    private static SourceType? ResolveResourceSource(SourceProductionContext context, DictionaryDocument document,
        XElement element, List<SourceType> sourceTypes)
    {
        string sourceName = ((string?)element.Attribute("source") ?? "").Trim();
        string? provider = ((string?)element.Attribute("provider"))?.Trim();
        if (sourceName.Length == 0 || provider is { Length: 0 })
        {
            string message = sourceName.Length == 0
                ? "An asset requires a nonempty 'source' attribute."
                : "The 'provider' attribute must contain the provider's assembly name.";
            Report(context, UnknownSource, document.Input, element, message);
            return null;
        }

        var candidates = sourceTypes.Where(source => string.Equals(source.Name, sourceName, StringComparison.OrdinalIgnoreCase));
        var matches = candidates.Where(source => provider is null ? source.IsBuiltIn :
            string.Equals(source.AssemblyName, provider, StringComparison.OrdinalIgnoreCase)).Take(2).ToArray();
        if (matches.Length == 1)
            return matches[0];

        string scope = provider is null ? "Sasogine" : $"provider '{provider}'";
        string diagnostic = matches.Length == 0
            ? $"Asset source '{sourceName}' was not found in {scope}."
            : $"Asset source '{sourceName}' is ambiguous in {scope}.";
        if (matches.Length == 0)
        {
            string availableProviders = string.Join(", ", candidates.Select(static source => source.AssemblyName)
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(static name => name, StringComparer.OrdinalIgnoreCase));
            if (availableProviders.Length > 0)
                diagnostic += $" Use 'provider' to select an assembly. Available providers: {availableProviders}.";
        }

        Report(context, UnknownSource, document.Input, element, diagnostic);
        return null;
    }

    private static List<SourceType> FindResourceSources(Compilation compilation, CancellationToken cancellationToken)
    {
        var result = new List<SourceType>();
        INamedTypeSymbol? resourceSource = compilation.GetTypeByMetadataName("Sachssoft.Engine.Resources.ResourceSourceBase");
        if (resourceSource is null || resourceSource.ContainingAssembly is not { } resourceAssembly)
            return result;

        INamedTypeSymbol? aliasAttribute = compilation.GetTypeByMetadataName("Sachssoft.Engine.Resources.ResourceSourceAttribute");
        var assemblies = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default) { compilation.Assembly, resourceAssembly };
        foreach (IAssemblySymbol reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            if (reference.Modules.Any(module => module.ReferencedAssemblySymbols.Any(
                dependency => SymbolEqualityComparer.Default.Equals(dependency, resourceAssembly))))
            {
                assemblies.Add(reference);
            }
        }

        foreach (IAssemblySymbol assembly in assemblies)
        foreach (INamedTypeSymbol type in EnumeratePublicTypes(assembly.GlobalNamespace, cancellationToken))
        {
            if (type.IsAbstract || !IsDerivedFrom(type, resourceSource) ||
                !type.InstanceConstructors.Any(static constructor => constructor.DeclaredAccessibility == Accessibility.Public &&
                    constructor.Parameters.Length == 1 && constructor.Parameters[0].RefKind == RefKind.None &&
                    constructor.Parameters[0].Type.SpecialType == SpecialType.System_String))
            {
                continue;
            }

            string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            bool isBuiltIn = SymbolEqualityComparer.Default.Equals(assembly, resourceAssembly);
            const string suffix = "Source";
            if (type.Name.Length > suffix.Length && type.Name.EndsWith(suffix, StringComparison.Ordinal))
                result.Add(new SourceType(type.Name.Substring(0, type.Name.Length - suffix.Length), typeName, assembly.Name, isBuiltIn));

            if (aliasAttribute is null)
                continue;
            foreach (AttributeData attribute in type.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, aliasAttribute) &&
                    attribute.ConstructorArguments.Length == 1 &&
                    attribute.ConstructorArguments[0].Value is string name && !string.IsNullOrWhiteSpace(name))
                {
                    result.Add(new SourceType(name, typeName, assembly.Name, isBuiltIn));
                }
            }
        }

        return result.GroupBy(static item => item.Name + "\u0000" + item.AssemblyName + "\u0000" + item.TypeName, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First()).ToList();
    }

    private static bool IsDerivedFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;
        return false;
    }

    private static IEnumerable<INamedTypeSymbol> EnumeratePublicTypes(INamespaceOrTypeSymbol owner, CancellationToken cancellationToken)
    {
        foreach (INamedTypeSymbol type in owner.GetTypeMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (type.DeclaredAccessibility != Accessibility.Public || type.Arity != 0)
                continue;

            yield return type;
            foreach (INamedTypeSymbol nested in EnumeratePublicTypes(type, cancellationToken))
                yield return nested;
        }

        if (owner is INamespaceSymbol ns)
        {
            foreach (INamespaceSymbol child in ns.GetNamespaceMembers())
            foreach (INamedTypeSymbol type in EnumeratePublicTypes(child, cancellationToken))
                yield return type;
        }
    }

    private static void AppendClass(StringBuilder output, ClassNode node, int depth, bool root)
    {
        string indent = new string(' ', depth * 4);
        output.Append(indent).Append("/// <summary>Contains ")
            .Append(root ? "the generated localization keys" : "localization keys")
            .AppendLine(" for this dictionary section.</summary>");
        output.Append(indent).Append("public static class ").Append(node.Name).AppendLine();
        output.Append(indent).AppendLine("{");
        foreach (var field in node.Fields)
        {
            output.Append(indent).Append("    /// <summary>Gets the localization entry for ")
                .Append(EscapeXml(field.Name)).AppendLine(".</summary>");
            output.Append(indent).Append("    public static readonly ").Append(field.Type)
                .Append(' ').Append(field.Name).Append(" = ").Append(field.Value).AppendLine(";");
        }
        foreach (var nested in node.Classes)
        {
            output.AppendLine();
            AppendClass(output, nested, depth + 1, false);
        }
        output.Append(indent).AppendLine("}");
    }

    private static string NameToIdentifier(string value)
    {
        var builder = new StringBuilder(value.Length + 1);
        bool upper = true;
        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character) || character == '_')
            {
                if (builder.Length == 0 && char.IsDigit(character)) builder.Append('_');
                builder.Append(upper ? char.ToUpperInvariant(character) : character);
                upper = false;
            }
            else upper = true;
        }
        if (builder.Length == 0) return "_";
        string result = builder.ToString();
        return SyntaxFacts.GetKeywordKind(result) != SyntaxKind.None ? "@" + result : result;
    }

    private static bool IsNamespaceIdentifier(string segment)
    {
        SyntaxToken token = SyntaxFactory.ParseToken(segment);
        return token.IsKind(SyntaxKind.IdentifierToken) && !token.ContainsDiagnostics && token.Text == segment;
    }

    private static string NormalizeNamespace(string proposed, string assembly)
    {
        string name = string.IsNullOrWhiteSpace(proposed) ? assembly : proposed;
        var segments = name.Split('.').Select(NameToIdentifier).ToArray();
        // RootNamespace already contains proper C# capitalization; do not normalize it unnecessarily.
        if (name.Split('.').All(static part => SyntaxFacts.IsValidIdentifier(part))) return name;
        return string.Join(".", segments);
    }

    private static string CSharpString(string value) => SyntaxFactory.Literal(value).Text;

    private static string EscapeXml(string value) => value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    private static void Report(SourceProductionContext context, DiagnosticDescriptor descriptor,
        LocalizationInput input, XElement? element, params object[] args)
    {
        int line = (element as IXmlLineInfo)?.HasLineInfo() == true ? ((IXmlLineInfo)element).LineNumber : 1;
        Report(context, descriptor, input, line, args);
    }

    private static void Report(SourceProductionContext context, DiagnosticDescriptor descriptor,
        LocalizationInput input, int? line, params object[] args)
    {
        Location location = Location.None;
        if (input.Text is not null)
        {
            int index = Math.Max(0, Math.Min(input.Text.Lines.Count - 1, (line ?? 1) - 1));
            TextLine sourceLine = input.Text.Lines[index];
            var span = new TextSpan(sourceLine.Start, 0);
            location = Location.Create(input.Path, span, input.Text.Lines.GetLinePositionSpan(span));
        }
        context.ReportDiagnostic(Diagnostic.Create(descriptor, location, args));
    }

    private static DiagnosticDescriptor Error(string id, string title, string message) =>
        new DiagnosticDescriptor(id, title, message, "Sasogine.Localization", DiagnosticSeverity.Error, isEnabledByDefault: true);

    private sealed class LocalizationInput
    {
        public readonly string Path;
        public readonly SourceText? Text;
        public LocalizationInput(string path, SourceText? text) { Path = path; Text = text; }
    }

    private sealed class DictionaryDocument
    {
        public readonly LocalizationInput Input;
        public readonly XElement Root;
        public readonly string Name;
        public readonly string Language;
        public readonly bool IsTemplate;
        public readonly string? Namespace;
        public readonly Dictionary<string, DictionaryEntry> Entries = new(StringComparer.Ordinal);

        public DictionaryDocument(LocalizationInput input, XElement root, string name, string language, bool isTemplate,
            string? declaredNamespace)
        {
            Input = input;
            Root = root;
            Name = name;
            Language = language;
            IsTemplate = isTemplate;
            Namespace = declaredNamespace;
        }
    }

    private sealed class DictionaryEntry
    {
        public readonly XElement Element;
        public readonly bool IsAsset;
        public DictionaryEntry(XElement element, bool isAsset) { Element = element; IsAsset = isAsset; }
    }

    private sealed class ClassNode
    {
        public readonly string Name;
        public readonly List<ClassNode> Classes = new();
        public readonly List<GeneratedField> Fields = new();
        public ClassNode(string name) { Name = name; }
    }

    private sealed class GeneratedField
    {
        public readonly string Name;
        public readonly string Type;
        public readonly string Value;
        public GeneratedField(string name, string type, string value) { Name = name; Type = type; Value = value; }
    }

    private sealed class SourceType
    {
        public readonly string Name;
        public readonly string TypeName;
        public readonly string AssemblyName;
        public readonly bool IsBuiltIn;

        public SourceType(string name, string typeName, string assemblyName, bool isBuiltIn)
        {
            Name = name;
            TypeName = typeName;
            AssemblyName = assemblyName;
            IsBuiltIn = isBuiltIn;
        }
    }
}
