using AmeisenBotRevamped.Logging.Enums;
using AmeisenBotRevamped.Logging.Objects;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AmeisenBotRevamped.Logging
{
    public class AmeisenBotLogger
    {
        private static readonly object padlock = new object();

        private static AmeisenBotLogger instance;
        public static AmeisenBotLogger Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    { instance = new AmeisenBotLogger(); }
                    return instance;
                }
            }
        }

        public bool Enabled { get; private set; }
        public LogLevel ActiveLogLevel { get; set; }
        public string LogFileFolder { get; private set; }
        public string LogFilePath { get; private set; }

        private Thread LogWorker { get; set; }
        private ConcurrentQueue<LogEntry> LogQueue { get; set; }

        private AmeisenBotLogger()
        {
            LogQueue = new ConcurrentQueue<LogEntry>();

            Enabled = true;
            ActiveLogLevel = LogLevel.Debug;
            ChangeLogFolder(AppDomain.CurrentDomain.BaseDirectory + "log/");
        }

        public void Start()
        {
            Enabled = true;

            if (LogWorker == null || !LogWorker.IsAlive)
            {
                LogWorker = new Thread(new ThreadStart(DoLogWork));
                LogWorker.Start();
            }
        }

        public void Stop()
        {
            Enabled = false;
            if (LogWorker.IsAlive)
            {
                LogWorker.Join();
            }
        }

        public void Log(string message, LogLevel logLevel = LogLevel.Debug, [CallerFilePath] string callingClass = "", [CallerMemberName]string callingFunction = "", [CallerLineNumber] int callingCodeline = 0)
        {
            if (logLevel <= ActiveLogLevel)
            {
                LogQueue.Enqueue(new LogEntry(logLevel, message, Path.GetFileNameWithoutExtension(callingClass), callingFunction, callingCodeline));
            }
        }

        public void ChangeLogFolder(string logFolderPath)
        {
            LogFileFolder = logFolderPath;
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }

            LogFilePath = LogFileFolder + $"AmeisenBot.{DateTime.Now.ToString("dd-MM-yyyy")}_{DateTime.Now.ToString("HH-mm")}.txt";
        }

        private void DoLogWork()
        {
            while (Enabled || !LogQueue.IsEmpty)
            {
                if (LogQueue.TryDequeue(out LogEntry activeEntry))
                {
                    File.AppendAllText(LogFilePath, activeEntry.ToString() + "\n");
                }

                Thread.Sleep(1);
            }
        }
    }
}
