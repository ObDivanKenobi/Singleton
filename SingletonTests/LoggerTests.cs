using Microsoft.VisualStudio.TestTools.UnitTesting;
using Singleton;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Singleton.Tests
{
    [TestClass()]
    public class LoggerTests
    {
        string logPath = "";
        string logFile = "testLog.txt";
        string flushAtQuantity = "3";

        string Path { get { return $"{logPath}{DateTime.Today.ToString("dd-MM-yyyy")}_{logFile}"; } }

        [TestMethod()]
        public void LogTest_OneThread()
        {
            SetConfiguration();
            string[] messages = { "message 1", "message 2" };

            ClearFile();
            Logger.Instance.Log(messages[0]);
            Logger.Instance.Log(messages[1]);

            bool ok = true;
            FileStream stream = new FileStream(Path, FileMode.Open, FileAccess.Read);
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream && ok)
                {
                    string line = reader.ReadLine();
                    ok = messages.Where(x => line.Contains(x)).Count() != 0;
                }
            }
            stream.Close();

            Assert.IsTrue(ok);
        }

        [TestMethod()]
        public void LogTest_Multythread()
        {
            SetConfiguration();
            string[] messages = { "thread 1 reporting!", "thread 1 reporting again!",
                "thread 2 reporting!", "oops, thread 3 failed!" };
            Task first = new Task(() =>
                {
                    Logger instance = Logger.Instance;

                    instance.Log(messages[0]);
                    Thread.Sleep(2000);
                    instance.Log(messages[1]);
                }),
                second = new Task(() =>
                {
                    Logger.Instance.Log(messages[2]);
                }),
                third = new Task(() =>
                {
                    try
                    {
                        throw new Exception(messages[3]);
                    }
                    catch(Exception e)
                    {
                        Logger.Instance.Log(e.Message);
                    }
                });

            ClearFile();
            first.Start();
            second.Start();
            third.Start();
            first.Wait();

            bool ok = true;
            FileStream stream = new FileStream(Path, FileMode.Open, FileAccess.Read);
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream && ok)
                {
                    string line = reader.ReadLine();
                    ok = messages.Where(x => line.Contains(x)).Count() != 0;
                }
            }
            stream.Close();

            Assert.IsTrue(ok);
        }

        void SetConfiguration()
        {
            ConfigurationManager.AppSettings["LogPath"] = logPath;
            ConfigurationManager.AppSettings["LogFile"] = logFile;
            ConfigurationManager.AppSettings["FlushAtQuantity"] = flushAtQuantity;
        }

        void ClearFile()
        {
            FileStream stream = new FileStream(Path, FileMode.Create, FileAccess.Write);
            stream.Close();
        }
    }
}