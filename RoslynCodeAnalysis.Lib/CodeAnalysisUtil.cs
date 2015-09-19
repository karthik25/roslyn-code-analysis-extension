using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeDeclarationSyntax = RoslynCodeAnalysis.Lib.Simplified.TypeDeclarationSyntax;

namespace RoslynCodeAnalysis.Lib
{
    public static class CodeAnalysisUtil
    {
        public static AnalysisDetails AnalyzeFile(this string fullPath)
        {
            var text = File.ReadAllText(fullPath);
            var tree = CSharpSyntaxTree.ParseText(text);
            var root = tree.GetRoot();

            var analysisDetails = new AnalysisDetails
                {
                    Classes = new List<TypeDeclarationSyntax>(),
                    Interfaces = new List<TypeDeclarationSyntax>()
                };

            var classTypes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classType in classTypes)
            {
                var isNestedNamespace = false;
                var nameSpace = classType.GetNamespace(out isNestedNamespace);
                var classInfo = new Simplified.TypeDeclarationSyntax
                {
                    NameSpace = nameSpace.Name.ToString(),
                    IsNested = isNestedNamespace,
                    Name = classType.Identifier.ToString(),
                    FileName = fullPath,
                    Modifiers = string.Join(",", classType.Modifiers.Select(m => m.Text).ToArray()),
                    SyntaxTree = classType,
                    MethodInfos = new List<Simplified.MethodDeclaraiontSyntax>(),
                    FieldInfos = new List<Simplified.FieldDeclaraiontSyntax>(),
                    PropertyInfos = new List<Simplified.PropertyDeclaraiontSyntax>()
                };
                var methodTypes = classType.DescendantNodes().OfType<MethodDeclarationSyntax>();
                foreach (var methodInfo in methodTypes.Select(methodType => new Simplified.MethodDeclaraiontSyntax
                    {
                        Name = methodType.Identifier.ToString(),
                        Modifiers = string.Join(",", methodType.Modifiers.Select(m => m.Text).ToArray()),
                        LineCount = methodType.Body != null ? methodType.Body.ToString().Split('\n').Count() : 0
                    }))
                {
                    classInfo.MethodInfos.Add(methodInfo);
                }
                var propertyTypes = classType.DescendantNodes().OfType<PropertyDeclarationSyntax>();
                foreach (var propInfo in propertyTypes.Select(propertyType => new Simplified.PropertyDeclaraiontSyntax
                    {
                        Name = propertyType.Identifier.ToString(),
                        Modifiers = string.Join(",", propertyType.Modifiers.Select(m => m.Text).ToArray())
                    }))
                {
                    classInfo.PropertyInfos.Add(propInfo);
                }
                var fieldTypes = classType.DescendantNodes().OfType<FieldDeclarationSyntax>();
                foreach (var fieldInfo in fieldTypes.Select(fieldType => new Simplified.FieldDeclaraiontSyntax
                    {
                        Name = fieldType.Declaration.ToString(),
                        Modifiers = string.Join(",", fieldType.Modifiers.Select(m => m.Text).ToArray())
                    }))
                {
                    classInfo.FieldInfos.Add(fieldInfo);
                }

                analysisDetails.Classes.Add(classInfo);
            }

            return analysisDetails;
        }
    }

    public class AnalysisDetails
    {
        public List<TypeDeclarationSyntax> Classes { get; set; }
        public List<TypeDeclarationSyntax> Interfaces { get; set; }
    }
}