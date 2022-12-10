using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime;
using UnityEngine;
using System.Text;

namespace QCommonLib
{
    public class QLogger
    {
        /// <summary>
        /// The calling assembly
        /// </summary>
        internal Assembly AssemblyObject { get; private set; }
        internal string AssemblyName { get => AssemblyObject.GetName().Name; }
        /// <summary>
        /// The full path and name of the log file
        /// </summary>
        internal string LogFile { get; private set; }
        /// <summary>
        /// NewLine for the player's environment
        /// </summary>
        internal string NL = Environment.NewLine;
        /// <summary>
        /// Runtime counter
        /// </summary>
        internal Stopwatch Timer { get; private set; }
        /// <summary>
        /// Log levels. Also output in log file.
        /// </summary>
        public enum LogLevel
        {
            Debug,
            Info,
            Error,
        }
        public enum LogLocation
        {
            Game = 1,
            Mod = 2,
            Both = 4
        }
        /// <summary>
        /// Which log file(s) minor messages are logged to
        /// </summary>
        internal LogLocation PreferredLocation { get; private set; }
        /// <summary>
        /// Should debug messages be logged?
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// Create QLogger instance
        /// </summary>
        /// <param name="isDebug">Override should debug messages be logged?</param>
        /// <param name="logFile">Override the generated path/file name</param>
        /// <param name="location">Override which log(s) minor messages are logged to</param>
        /// <exception cref="ArgumentNullException"></exception>
        public QLogger(bool isDebug = false, string logFile = "", LogLocation location = LogLocation.Mod)
        {
            AssemblyObject = Assembly.GetCallingAssembly() ?? throw new ArgumentNullException("QLogger: Failed to find calling assembly");
            LogFile = logFile == "" ? Path.Combine(Application.dataPath, AssemblyName + ".log") : logFile;
            IsDebug = isDebug;
            PreferredLocation = location;
            Timer = Stopwatch.StartNew();

            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }

            AssemblyName details = AssemblyObject.GetName();
            string offset;
            try
            {
                TimeSpan ts = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now); //TimeZoneInfo.Local.BaseUtcOffset;
                offset = string.Format("{0}:{1:D2}", ts.Hours, ts.Minutes);
            }
            catch (Exception)
            {
                offset = "Unknown";
            }
            Info($"{details.Name} v{details.Version} at " + DateTime.UtcNow.ToString(new CultureInfo("en-GB")) + $" ({offset})");
        }

        ~QLogger()
        {
            Info($"{AssemblyName} closing (" + DateTime.UtcNow.ToString(new CultureInfo("en-GB")) + ")");
        }

        #region Debug
        // Print the message to LogLocation only if IsDebug is true
        public void Debug(string message, string code = "")
        {
            if (IsDebug)
            {
                Do(message, LogLevel.Debug, code);
            }
        }

        public void Debug(Exception exception, string code = "")
        {
            if (IsDebug)
            {
                Do(exception.ToString(), LogLevel.Debug, code);
            }
        }
        #endregion

        #region Info
        // Print the message to LogLocation
        public void Info(string message, string code = "")
        {
            Do(message, LogLevel.Info, code);
        }

        public void Info(Exception exception, string code = "")
        {
            Do(exception.ToString(), LogLevel.Info, code);
        }
        #endregion

        #region Warning
        // Print the message everywhere
        public void Warning(string message, string code = "")
        {
            Do(message, LogLevel.Error, code);
        }

        public void Warning(Exception exception, string code = "")
        {
            Do(exception.ToStringNoTrace(), LogLevel.Error, code);
        }
        #endregion

        #region Error
        // Print the message everywhere, include stacktrace
        public void Error(string message, string code = "")
        {
            Do(message + NL + new StackTrace().ToString() + NL, LogLevel.Error, code);
        }

        public void Error(Exception exception, string code = "")
        {
            string message = exception.ToString();
            if (exception.StackTrace is null || exception.StackTrace == "") message += NL + new StackTrace().ToString();
            Do(message, LogLevel.Error, code);
        }
        #endregion

        internal void Do(string message, LogLevel logLevel, string code)
        {
            try
            {
                var ticks = Timer.ElapsedTicks;
                string msg = "";
                if (code != "") code += " ";

                int maxLen = Enum.GetNames(typeof(LogLevel)).Select(str => str.Length).Max();
                msg += string.Format($"{{0, -{maxLen}}}", $"[{logLevel}] ");

                long secs = ticks / Stopwatch.Frequency;
                long fraction = ticks % Stopwatch.Frequency;
                msg += string.Format($"{secs:n0}.{fraction:D7} | {code}{message}{NL}");

                if ((PreferredLocation & LogLocation.Mod) == LogLocation.Mod)
                {
                    using (StreamWriter w = File.AppendText(LogFile))
                    {
                        w.Write(msg);
                    }
                }

                if (logLevel == LogLevel.Error || ((PreferredLocation & LogLocation.Game) == LogLocation.Game))
                {
                    msg = AssemblyName + " | " + msg;
                    switch (logLevel)
                    {
                        case LogLevel.Error:
                            UnityEngine.Debug.LogError(msg);
                            break;
                        default:
                            UnityEngine.Debug.Log(msg);
                            break;
                    }
                }
            }
            catch
            {
                UnityEngine.Debug.Log("QLogger failed to log!");
            }
        }
    }

    public static class MyExtensions
    {
        public static string ToStringNoTrace(this Exception e)
        {
            StringBuilder stringBuilder = new StringBuilder(e.GetType().ToString());
            stringBuilder.Append(": ").Append(e.Message);
            return stringBuilder.ToString();
        }

        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }
    }
}
