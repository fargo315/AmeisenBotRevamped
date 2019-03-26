using AmeisenBotRevamped.Logging.Enums;
using System;
using System.IO;

namespace AmeisenBotRevamped.Logging.Objects
{
    public class LogEntry
    {
        public DateTime TimeStamp { get; private set; }
        public string Message { get; private set; }
        public LogLevel LogLevel { get; private set; }

        public string CallingClass { get; private set; }
        public string CallingFunction { get; private set; }
        public int CallingCodeline { get; private set; }

        public LogEntry(LogLevel logLevel, string message, string callingClass, string callingFunction, int callingCodeline)
        {
            TimeStamp = DateTime.Now;
            LogLevel = logLevel;
            Message = message;
            CallingClass = callingClass;
            CallingFunction = callingFunction;
            CallingCodeline = callingCodeline;
        }

        public override string ToString()
        {
            return $"[{TimeStamp.ToLongTimeString()}]\t[{LogLevel.ToString()}]\t[{CallingClass}:{CallingCodeline}:{CallingFunction}]\t{Message}";
        }
    }
}
