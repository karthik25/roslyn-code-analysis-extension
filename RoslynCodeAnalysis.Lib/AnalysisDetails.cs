using System.Collections.Generic;
using RoslynCodeAnalysis.Lib.Simplified;

namespace RoslynCodeAnalysis.Lib
{
    public class AnalysisDetails
    {
        public List<TypeDeclarationSyntax> Classes { get; set; }
        public List<TypeDeclarationSyntax> Interfaces { get; set; }
    }
}