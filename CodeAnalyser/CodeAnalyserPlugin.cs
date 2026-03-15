using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

public class CodeAnalyserPlugin
{
    [KernelFunction]
    [Description("Analyses a C# file and extracts all method signatures and class names")]
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

        var classes = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>();

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

                sb.AppendLine($"  Method: {method.Identifier.Text}({parameters}) -> {method.ReturnType}");
            }
        }

        if (sb.Length == 0)
            return "No classes or methods found in this file.";

        return sb.ToString();
    }
}