using Humanizer;

namespace RoslynCodeAnalysis
{
    public static class RoslynCodeAnalysisFluentExtensions
    {
        public static int Cycle(this int src, int max)
        {
            var cycled = src + 1;
            return cycled < max ? cycled : 0;
        }

        public static string Pluralize(this int number, string text)
        {
            return number == 1 ? string.Format("{0} {1}", number, text) 
                               : string.Format("{0} {1}", number, text.Pluralize());
        }
    }
}