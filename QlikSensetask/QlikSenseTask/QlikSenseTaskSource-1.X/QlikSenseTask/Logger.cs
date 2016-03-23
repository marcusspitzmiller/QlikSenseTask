using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLogger
{
    public static class EnumHelper
    {
        /*public static T FromInt<T>(int value)
        {
            return (T)value;
        }*/

        public static T FromString<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }

    public enum LogLevel { Debug, Information, Warning, Error, Fatal }; 

    class Logger
    {
        string sLogFile = "";
        LogLevel minlevel = LogLevel.Debug;
        System.IO.StreamWriter file = null;

        

        public void Start()
        {
            DateTime d = DateTime.Now;

            sLogFile = "QlikSenseTasklog_" + d.ToString("yyyy_MM_dd_hh_mm_ss") + ".log";
            
            file = new System.IO.StreamWriter(sLogFile);

            file.WriteLine("Time, LogLevel, Message");
        }

        public void End()
        {
            file.Close();
        }

        public void SetLogLevel(LogLevel loglevel)
        {
            minlevel = loglevel;
        }

        public void Log(LogLevel loglevel, string message)
        {
            if (loglevel >= minlevel)
            {
                DateTime d = DateTime.Now;

                string line = d.ToString("yyyy_MM_dd_hh_mm_ss") + ",";
                line += loglevel.ToString() + ",";
                line += (char)34 + message + (char)34;

                Console.WriteLine(line);
                file.WriteLine(line);
            }
        }

        public string GetLogFilePath()
        {
            file.Flush();
            return Path.GetFullPath(sLogFile);
        }
    }
}
