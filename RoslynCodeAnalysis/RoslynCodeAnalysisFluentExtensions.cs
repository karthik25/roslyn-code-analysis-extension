namespace RoslynCodeAnalysis
{
    public static class RoslynCodeAnalysisFluentExtensions
    {
        public static int Cycle(this int src, int max)
        {
            var cycled = src + 1;
            return cycled < max ? cycled : 0;
        }
    }
}