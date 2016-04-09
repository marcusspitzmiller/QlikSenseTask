using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

using QlikSenseJSONObjects;
using System.Diagnostics;
using MyLogger;

namespace QlikSenseTask
{
    enum TaskStatus { NeverStarted, Triggered, Started, Queued, AbortInitiated, Aborting, Aborted, Success, Failed, Skipped, Retrying, Error, Reset };

    class Program
    {
        //private QlikSenseJSONHelper qs;

        static void Main(string[] args)
        {
            //usage****
            //-proxy:https://usral-msi  -timeout:10000 -task:"test123" -wait
            //***
            Console.WriteLine("Use at your own risk.  Created by Marcus Spitzmiller.");
            Console.WriteLine("");

            Logger logger = new Logger();
            logger.Start();
            logger.SetLogLevel(LogLevel.Debug);


            //defaults
            string proxy = "";
            Int32 timeout = 600;
            Int32 poll = 30;
            string task = "";
            bool synchronous = true;

            //global settings from config.txt
            string[] global = { };
            if (File.Exists("config.txt"))
            {
                global = File.ReadAllLines("config.txt");
                //args.CopyTo(global, 0);
            }

            string [] allargs = new string[args.Length + global.Length];
            global.CopyTo(allargs, 0);
            args.CopyTo(allargs, global.Length);

            for (int i = 0; i < allargs.Length; i++)
            {
                string arg = allargs[i];
                string[] param = arg.Split(':');

                switch (param[0])
                {
                    case "-?":
                    case "/?":
                        Console.WriteLine("Usage:");
                        Console.WriteLine("-proxy:<URL address of proxy>  required example https://server.company.com");
                        Console.WriteLine(@"-task:<taskname>               required, example ""test 123""");
                        Console.WriteLine("-wait:<# seconds to wait>      optional, default 600");
                        Console.WriteLine("-poll:<polling frequency in # seconds>      optional, default 30");
                        Console.WriteLine("   omit -wait to return immediately");
                        Console.WriteLine("   use -wait to wait for the task to finish");
                        Console.WriteLine("     Return Codes:");
                        Console.WriteLine("     0 - task completed successfully");
                        Console.WriteLine("     4 - task timed out");
                        Console.WriteLine("     8 - task failed");
                        Console.WriteLine("");
                        Console.WriteLine("Optionally define any or all parameters in config.txt");
                        Console.WriteLine("  (to be used globally for all tasks)");
                        Console.WriteLine("");
                        Console.WriteLine("Example usage:");
                        Console.WriteLine("qliksensetask -task:\"Reload License Monitor\" -proxy:https://localhost -wait:600");
                        Console.WriteLine("");
                       // Console.WriteLine("-debug                         optional");
                       // Console.WriteLine("   omit -wait to return immediately");
                        Console.WriteLine("press any key...");
                        Console.ReadKey();
                        Environment.Exit(0);
                        break;
                    case "-proxy":
                        proxy = "";
                        for (int j = 1; j < param.Length; j++)
                        {
                            proxy += param[j];
                            if (j < param.Length - 1) proxy += ":"; //put back the colon
                        }
                        
                        break;
                    case "-wait":
                        timeout = Convert.ToInt32(param[1]);
                        
                        break;
                    case "-poll":
                        poll = Convert.ToInt32(param[1]);
                        break;
                    case "-task":
                        task = param[1];
                        logger.Log(LogLevel.Information, "Task: " + task);
                        
                        break;
                    default:
                        logger.Log(LogLevel.Information, "Unrecognized: " + param[0]);
                        
                        break;
                        
                }

            }


            logger.Log(LogLevel.Information, "Proxy: " + proxy);
            logger.Log(LogLevel.Information, "Wait: " + timeout + " seconds");
            logger.Log(LogLevel.Information, "Poll: " + poll + " seconds");

            if (proxy == "" || task == "")
            {
                logger.Log(LogLevel.Fatal, "Proxy or Task undefined");
                
                Environment.Exit(4);
            }

            int retval = 0;
            try
            {
                QlikSenseJSONHelper qs = new QlikSenseJSONHelper(proxy, 60000, logger); //http timeout 60 seconds

                logger.Log(LogLevel.Information, "Task is starting...");
                QlikSenseTaskExecutionGuid taskexecutionguid = qs.StartTaskByName(task, synchronous);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                TaskStatus status = (TaskStatus)qs.GetTaskStatusByName(task);   //1 is running, 3 is success
                //while (status == 1 && stopwatch.Elapsed.Seconds < timeout) //running  //1.x
                while((status == TaskStatus.NeverStarted || status == TaskStatus.Queued || 
                      status == TaskStatus.Retrying || status == TaskStatus.Started || status == TaskStatus.Triggered) && stopwatch.Elapsed.Seconds < timeout)
                {
                    Thread.Sleep(poll * 1000);    //wait 30 seconds and try again

                    logger.Log(LogLevel.Information, "Task is running...");
                    status = (TaskStatus)qs.GetTaskStatusByName(task);   //1 is running, 3 is success
                }

                if (stopwatch.Elapsed.Seconds > timeout)
                {
                    logger.Log(LogLevel.Information, "Task timeout with status: " + status.ToString());
                }
                else
                {
                    logger.Log(LogLevel.Information, "Task finished with status: " + status.ToString());
                }
                stopwatch.Stop();
                

                if (status == TaskStatus.Success)    //success
                {
                    
                }
                else if (status == TaskStatus.Started)        //still running
                {
                    retval = 4;
                }
                else //problem
                {
                    retval = 8;
                }
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, e.ToString());
                
                if (e.ToString() == "Timeout: The operation has timed out")
                {
                    retval = 4;
                }
                else
                {
                    retval = 8;
                }
            }

            logger.Log(LogLevel.Information, "Returning Errorlevel " + retval.ToString());

            logger.End();
            Environment.Exit(retval);
        }
    }
}
