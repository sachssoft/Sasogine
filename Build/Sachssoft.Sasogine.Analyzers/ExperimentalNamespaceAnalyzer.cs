using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Immutable;

namespace Sachssoft.Sasogine.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExperimentalAssemblyAnalyzer : DiagnosticAnalyzer
{
    private const string ExperimentalAssemblyPrefix =
        "Sachssoft.Sasogine.Experimental";

    private static readonly DiagnosticDescriptor Rule = new(
        id: "SASO001",
        title: "Experimental API used",
        messageFormat:
            "API '{0}' belongs to experimental assembly '{1}' and must not be used by assembly '{2}'",
        category: "Architecture",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var currentAssemblyName =
                compilationContext.Compilation.AssemblyName;

            // Experimental assemblies may freely use other experimental assemblies.
            if (IsExperimentalAssembly(currentAssemblyName))
                return;

            compilationContext.RegisterOperationAction(
                AnalyzeOperation,
                OperationKind.ObjectCreation,
                OperationKind.Invocation,
                OperationKind.FieldReference,
                OperationKind.PropertyReference,
                OperationKind.EventReference,
                OperationKind.MethodReference,
                OperationKind.TypeOf,
                OperationKind.Conversion);
        });
    }

    private static void AnalyzeOperation(
        OperationAnalysisContext context)
    {
        var symbol = GetReferencedSymbol(context.Operation);

        if (symbol is not null)
        {
            var assembly = GetAssembly(symbol);

            if (assembly is not null &&
                IsExperimentalAssembly(assembly.Name))
            {
                Report(context, symbol, assembly);
                return;
            }
        }

        if (context.Operation.Type is { } type)
        {
            var assembly = type.ContainingAssembly;

            if (assembly is not null &&
                IsExperimentalAssembly(assembly.Name))
            {
                Report(context, type, assembly);
            }
        }
    }

    private static ISymbol? GetReferencedSymbol(IOperation operation)
    {
        return operation switch
        {
            IObjectCreationOperation x => x.Constructor,
            IInvocationOperation x => x.TargetMethod,
            IFieldReferenceOperation x => x.Field,
            IPropertyReferenceOperation x => x.Property,
            IEventReferenceOperation x => x.Event,
            IMethodReferenceOperation x => x.Method,
            _ => null
        };
    }

    private static IAssemblySymbol? GetAssembly(ISymbol symbol)
    {
        return symbol switch
        {
            ITypeSymbol type => type.ContainingAssembly,
            _ => symbol.ContainingAssembly ??
                 symbol.ContainingType?.ContainingAssembly
        };
    }

    private static bool IsExperimentalAssembly(string? assemblyName)
    {
        if (string.IsNullOrEmpty(assemblyName))
            return false;

        return assemblyName.Equals(
                   ExperimentalAssemblyPrefix,
                   StringComparison.Ordinal) ||
               assemblyName.StartsWith(
                   ExperimentalAssemblyPrefix + ".",
                   StringComparison.Ordinal);
    }

    private static void Report(
        OperationAnalysisContext context,
        ISymbol symbol,
        IAssemblySymbol assembly)
    {
        context.ReportDiagnostic(
            Diagnostic.Create(
                Rule,
                context.Operation.Syntax.GetLocation(),
                symbol.ToDisplayString(),
                assembly.Name,
                context.Compilation.AssemblyName));
    }
}