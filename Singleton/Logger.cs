using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singleton
{
    public class Logger : IDisposable
    {
        private static readonly Lazy<Logger> _instance = 
            new Lazy<Logger>(() => new Logger());
        public static Logger Instance { get { return _instance.Value; } }

        private string logPath;
        private string logFile;
        private Queue<(string Message, DateTime Time)> logQueue;
        private int flushAtQuantity = 1;

        public string FullPath { get { return $"{logPath}{DateTime.Today.ToString("dd-MM-yyyy")}_{logFile}"; } }

        private Logger()
        {
            logQueue = new Queue<(string, DateTime)>();
            logPath = ConfigurationManager.AppSettings["LogPath"];
            logFile = ConfigurationManager.AppSettings["LogFile"];
            flushAtQuantity = int.Parse(ConfigurationManager.AppSettings["FlushAtQuantity"]);
        }

        ~Logger()
        {
            Dispose();
        }

        public void Dispose()
        {
            FlushToFile();
        }

        public void Log(string message)
        {
            lock(logQueue)
            {
                logQueue.Enqueue((message, DateTime.Now));

                if (logQueue.Count >= flushAtQuantity)
                    FlushToFile();
            }
        }

        private void FlushToFile()
        {
            FileStream stream = new FileStream(FullPath, FileMode.Append, FileAccess.Write);
            using (var writer = new StreamWriter(stream))
                while (logQueue.Count > 0)
                {
                    var entry = logQueue.Dequeue();
                    writer.WriteLine($"{entry.Time}\t{entry.Message}");
                }
            stream.Close();
        }
    }
}
