﻿using System.Collections.Generic;

namespace RoslynCodeAnalysis.Lib.Simplified
{
    public class TypeDeclarationSyntax : BaseDeclarationSyntax
    {
        public string NameSpace { get; set; }
        public bool IsNested { get; set; }
        public string FileName { get; set; }
        public List<PropertyDeclaraiontSyntax> PropertyInfos { get; set; }
        public List<MethodDeclaraiontSyntax> MethodInfos { get; set; }
        public List<FieldDeclaraiontSyntax> FieldInfos { get; set; }
        public Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax SyntaxTree { get; set; }
        public bool IsInterface { get; set; }
    }
}