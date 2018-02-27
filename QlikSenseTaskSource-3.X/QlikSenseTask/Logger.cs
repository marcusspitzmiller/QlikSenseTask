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

        

        public void Start(string task, string logpath)
        {
            DateTime d = DateTime.Now;
            Boolean dirtest = Directory.Exists(logpath);
            Directory.CreateDirectory(logpath);         
            
            sLogFile = logpath + "\\" + task + "_" + d.ToString("yyyy_MM_dd_HH_mm_ss") + ".log";
            
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

                string line = d.ToString("yyyy_MM_dd_HH_mm_ss") + ",";
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
        public void CleanLogFiles(string logpath, Int32 deletelog)
        {
            DirectoryInfo historyDi = new DirectoryInfo(logpath);

            //remove any history files greater than 90 days
            //DateTime from_date = DateTime.Now.AddMonths(-3);
            DateTime from_date = DateTime.Now.AddDays(deletelog);
            List<FileInfo> historyFileInfoList = historyDi.GetFiles().Where(file => file.LastWriteTime < from_date).ToList<FileInfo>();
            foreach (FileInfo historyFile in historyFileInfoList)
            {
                string historytemp = historyFile.Extension;
                if (historyFile.Extension == ".log")
                {
                    Log(LogLevel.Information, "Deleting Log File: " + historyFile.Name);
                    string historytemp2 = historyFile.FullName;
                    DateTime historytemp3 = historyFile.LastWriteTime;
                    try
                    {
                        historyFile.Delete();
                    }
                    catch (Exception e)
                    {
                        Log(
                            LogLevel.Error,
                            String.Format("Failed to delete log file: {0}\nError Message: {1}", historyFile.Name, e.Message)
                        );
                    }
                }
            }
        }
    }
}
