using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynCodeAnalysis.Lib
{
    public static class SyntaxTreeHelpers
    {
        public static NamespaceDeclarationSyntax GetNamespace(this SyntaxNode classDeclarationSyntax, out bool isNested)
        {
            var parentNode = classDeclarationSyntax.Parent;
            isNested = parentNode.Parent is NamespaceDeclarationSyntax;
            var ns = parentNode as NamespaceDeclarationSyntax;
            return ns ?? parentNode.GetNamespace(out isNested);
        }
    }
}
