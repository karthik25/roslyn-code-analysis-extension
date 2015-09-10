using System;

namespace RoslynCodeAnalysis.Contracts
{
    public interface ILogger
    {
        void WriteLog(string message, params object[] args);
        void WriteLog(Exception exception);
    }
}
