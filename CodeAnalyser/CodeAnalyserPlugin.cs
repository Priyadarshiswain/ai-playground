using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

public class CodeAnalyserPlugin
{
    [KernelFunction]
    [Description("Analyses a C# file and extracts classes, methods, complexity, dependencies and TODO comments")]
    public string AnalyseCode(
        [Description("The path to the C# file to analyse")]
        string filePath)
    {
        if (!File.Exists(filePath))
            return $"File not found: {filePath}";

        var code = File.ReadAllText(filePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetCompilationUnitRoot();

        var sb = new StringBuilder();

        // Dependencies
        var usings = root.Usings
            .Select(u => u.Name?.ToString())
            .Where(u => u != null)
            .ToList();

        if (usings.Any())
        {
            sb.AppendLine("=== Dependencies ===");
            foreach (var u in usings)
                sb.AppendLine($"  {u}");
            sb.AppendLine();
        }

        var usedTypes = GetUsedTypes(root);
        if (usedTypes.Any())
        {
            sb.AppendLine("=== Used Types ===");
            foreach (var type in usedTypes)
                sb.AppendLine($"  {type}");
            sb.AppendLine();
        }
        // TODO: Add support for interface analysis

        // TODO comments
        var todos = tree.GetRoot()
            .DescendantTrivia()
            .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) &&
                        t.ToString().Contains("TODO", StringComparison.OrdinalIgnoreCase))
            .Select(t => t.ToString().Trim())
            .ToList();

        if (todos.Any())
        {
            sb.AppendLine("=== TODO Comments ===");
            foreach (var todo in todos)
                sb.AppendLine($"  {todo}");
            sb.AppendLine();
        }

        // Classes and methods
        var classes = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>();

        sb.AppendLine("=== Classes & Methods ===");

        foreach (var cls in classes)
        {
            sb.AppendLine($"Class: {cls.Identifier.Text}");

            var methods = cls.DescendantNodes()
                .OfType<MethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                var parameters = string.Join(", ",
                    method.ParameterList.Parameters
                        .Select(p => $"{p.Type} {p.Identifier}"));

                var lineCount = method.Body?.Statements.Count ?? 0;
                var complexity = CalculateCyclomaticComplexity(method);

                sb.AppendLine($"  Method: {method.Identifier.Text}({parameters}) -> {method.ReturnType}");
                sb.AppendLine($"    Lines: {lineCount} | Complexity: {complexity} {ComplexityLabel(complexity)}");
            }
        }

        return sb.ToString();
    }

    private int CalculateCyclomaticComplexity(MethodDeclarationSyntax method)
    {
        var complexity = 1;
        var nodes = method.DescendantNodes();

        complexity += nodes.OfType<IfStatementSyntax>().Count();
        complexity += nodes.OfType<ElseClauseSyntax>().Count();
        complexity += nodes.OfType<ForStatementSyntax>().Count();
        complexity += nodes.OfType<ForEachStatementSyntax>().Count();
        complexity += nodes.OfType<WhileStatementSyntax>().Count();
        complexity += nodes.OfType<DoStatementSyntax>().Count();
        complexity += nodes.OfType<SwitchSectionSyntax>().Count();
        complexity += nodes.OfType<ConditionalExpressionSyntax>().Count();
        complexity += nodes.OfType<CatchClauseSyntax>().Count();

        return complexity;
    }

    private string ComplexityLabel(int complexity) => complexity switch
    {
        <= 5 => "(simple)",
        <= 10 => "(moderate)",
        <= 20 => "(complex)",
        _ => "(REFACTOR THIS)"
    };

    private List<string> GetUsedTypes(CompilationUnitSyntax root)
    {
        // Get declared class names to exclude them
        var declaredClasses = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Select(c => c.Identifier.Text)
            .ToHashSet();

        // Known LINQ and common method names to exclude
        var exclude = new HashSet<string>
        {
            "Any", "All", "Where", "Select", "ToList", "ToArray",
            "FirstOrDefault", "First", "Count", "OrderBy", "OrderByDescending",
            "GroupBy", "Distinct", "Join", "Contains", "Skip", "Take",
            "AppendLine", "ToString", "Trim", "Split", "Replace",
            "GetRoot", "ParseText", "ReadAllText", "DescendantNodes",
            "DescendantTrivia", "IsKind", "IsUpper"
        };

        return root.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Select(i => i.Identifier.Text)
            .Where(name =>
                char.IsUpper(name[0]) &&
                name.EndsWith("Syntax") ||
                name.EndsWith("Exception") ||
                name.EndsWith("Builder") ||
                name.EndsWith("Config") ||
                name.EndsWith("Client") ||
                name.EndsWith("Service") ||
                name.EndsWith("Handler") ||
                name.EndsWith("Repository") ||
                name.EndsWith("Plugin") ||
                name.EndsWith("Attribute") ||
                name == "StringBuilder" ||
                name == "StringComparison" ||
                name == "SyntaxKind" ||
                name == "File" ||
                name == "Path"
            )
            .Except(exclude)
            .Except(declaredClasses)
            .Distinct()
            .OrderBy(name => name)
            .ToList();
    }
}