using System;
using System.ComponentModel.Composition;
using System.IO;
using RoslynCodeAnalysis.Contracts;

namespace RoslynCodeAnalysis.SimpleLogger
{
    [Export(typeof(ILogger))]
    public class TextLogger : ILogger
    {
        private static readonly string LogPath = string.Format(@"{0}\RoslynLog.txt",
                                                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        public void WriteLog(string message, params object[] args)
        {
            WriteToFile(string.Format(message, args));
        }

        public void WriteLog(Exception exception)
        {
            WriteToFile(exception.ToString());
        }

        private static void WriteToFile(string data)
        {
            var logStream = new StreamWriter(LogPath, true);
            logStream.WriteLine(data);
            logStream.Close();
        }
    }
}
