using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QVnextDemoBuilder;
using Newtonsoft.Json;
//using Qlik.Engine;
using MyLogger;

namespace QlikSenseJSONObjects
{
    class QlikSenseJSONHelper
    {
        QRSNTLMWebClient qrsClient;
        Logger logger;
        
        string url;

        public QlikSenseJSONHelper(string url, Int32 timeout, Logger logger)
        {
            this.url = url;
            
            qrsClient = new QRSNTLMWebClient(url, timeout);

            this.logger = logger;

        }

        public string GetAppID(string appname)
        {
            Dictionary<string, string> appqueries = new Dictionary<string, string>();
            appqueries.Add("filter", "name eq '" + appname + "'");

            //find the app
            string appid = "";
            string appsstring = qrsClient.Get("/qrs/app", appqueries);
            List<QlikSenseApp> apps = (List<QlikSenseApp>)JsonConvert.DeserializeObject<List<QlikSenseApp>>(appsstring);
            for (int i = 0; i < apps.Count; i++)
            {
                if (apps[i].name == appname)
                {
                    appid = apps[i].id;
                    
                }
            }

            if (appid == "")
            {
                throw new Exception("Couldn't find app");
            }
            return appid;
        }

        public int GetTaskStatusByName(string taskname)
        {
            Dictionary<string, string> queries = new Dictionary<string, string>();
            queries.Add("filter", "name eq '" + taskname + "'");

            //find the app
            string taskid = "";
            string taskstring = qrsClient.Get("/qrs/task", queries);
            List<QlikSenseTaskResult> tasks = (List<QlikSenseTaskResult>)JsonConvert.DeserializeObject<List<QlikSenseTaskResult>>(taskstring);

            int retval = -1;
            if (tasks.Count == 1)
            {
                retval = tasks[0].operational.lastExecutionResult.status;
            }

            return retval;
        }

        public int GetTaskStatus(QlikSenseTaskExecutionGuid taskexecutionguid)
        {
            Dictionary<string, string> queries = new Dictionary<string, string>();
            //queries.Add("filter", "name eq '" + taskname + "'");

            //find the app
            string taskid = "";
            string taskstring = qrsClient.Get("/qrs/executionresult/"+taskexecutionguid.value, queries);
            List<QlikSenseTaskResult> tasks = (List<QlikSenseTaskResult>)JsonConvert.DeserializeObject<List<QlikSenseTaskResult>>(taskstring);

            int retval = 0;
            if (tasks.Count == 1)
            {
                retval = tasks[0].operational.lastExecutionResult.status;
            }

            return retval;
        }


        public string GetStreamID(string streamname)
        {
            Dictionary<string, string> streamqueries = new Dictionary<string, string>();
            streamqueries.Add("filter", "name eq '" + streamname + "'");

            //find the stream
            string streamid = "";
            string streamstring = qrsClient.Get("/qrs/stream", streamqueries);
            List<QlikSenseStream> streams = (List<QlikSenseStream>)JsonConvert.DeserializeObject<List<QlikSenseStream>>(streamstring);
            for (int i = 0; i < streams.Count; i++)
            {
                if (streams[i].name == streamname)
                {
                    streamid = streams[i].id;

                }
            }

            if (streamid == "")
            {
                throw new Exception("Couldn't find stream");
            }
            return streamid;
        }

        public QlikSenseApp GetApp(string appname)
        {
            Dictionary<string, string> appqueries = new Dictionary<string, string>();
            appqueries.Add("filter", "name eq '" + appname + "'");

            //find the app
            string appsstring = qrsClient.Get("/qrs/app", appqueries);
            List<QlikSenseApp> apps = (List<QlikSenseApp>)JsonConvert.DeserializeObject<List<QlikSenseApp>>(appsstring);
            for (int i = 0; i < apps.Count; i++)
            {
                if (apps[i].name == appname)
                {
                    return apps[i];
                }
            }

            return null;
        }

        public string CopyApp(string appid, string name)
        {
            string endpoint = "/qrs/app/" + appid + "/copy";
            string newappjson = "";
            if (name != "")
            {
                Dictionary<string, string> queries = new Dictionary<string, string>();
                queries.Add("name", name);
                newappjson = qrsClient.Post(endpoint, queries);
            }
            else
            {
                newappjson = qrsClient.Post(endpoint, "");
            }
            QlikSenseApp newapp = (QlikSenseApp)JsonConvert.DeserializeObject<QlikSenseApp>(newappjson);
            return newapp.id;
        }

        public void Reload(string appid)
        {
            qrsClient.Post("/qrs/app/" + appid + "/reload", "");
        }

        public void StartTask(string taskid, bool synchronous)
        {
            string path = "/qrs/task/" + taskid + "/start";
            if (synchronous) path += "/synchronous";

            Dictionary<string, string> queries = new Dictionary<string, string>();
            //queries.Add("name", taskname);

            //try
            //{
                qrsClient.Post(path, "");
            //}
            //catch(Exception e)
            //{
                
            //}
        }

        public QlikSenseTaskExecutionGuid StartTaskByName(string taskname, bool synchronous)
        {
            int retval = 0;

            string path = "/qrs/task/start";
            if (synchronous) path += "/synchronous";
            
            Dictionary<string, string> queries = new Dictionary<string, string>();
            queries.Add("name", taskname);

            string newtaskjsonout = "";
            try
            {
                newtaskjsonout = qrsClient.Post(path, queries);
            }
            catch (Exception e)
            {
                throw e;
            }

            return (QlikSenseTaskExecutionGuid)JsonConvert.DeserializeObject<QlikSenseTaskExecutionGuid>(newtaskjsonout);
            //should change to use the execution result API
        }

        /*public void CheckTask(string guid)
        {
            string path = "/qrs/task/table";

            Dictionary<string, string> queries = new Dictionary<string, string>();
            queries.Add("filter", "id eq '" + guid + "'");

            string newtaskjsonout = qrsClient.Post(path, queries);
            QlikSenseTaskGuid result = (QlikSenseTaskGuid)JsonConvert.DeserializeObject<QlikSenseTaskGuid>(newtaskjsonout);
        }*/

        public void Publish(string appid, string streamid)
        {
            try
            {
                Dictionary<string, string> appqueries = new Dictionary<string, string>();
                appqueries.Add("stream", streamid);

                qrsClient.Put("/qrs/app/" + appid + "/publish", appqueries);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Replace(string newappid, string oldappid)
        {
            try
            {
                Dictionary<string, string> appqueries = new Dictionary<string, string>();
                appqueries.Add("app", oldappid);
                string result = qrsClient.Post("/qrs/app/" + newappid + "/replace", "");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Delete(string appid)
        {
            qrsClient.Delete("/qrs/app/" + appid);
        }

        /*private IApp GetAppHandle(string appid)
        {
            Uri uri = new Uri(url);
            ILocation remoteQlikSenseLocation = Qlik.Engine.Location.FromUri(uri);
            remoteQlikSenseLocation.AsNtlmUserViaProxy();
            IAppIdentifier appidentifier = remoteQlikSenseLocation.AppWithId(appid);
            return remoteQlikSenseLocation.App(appidentifier);
        }*/

        public string CreateTask(string appid, string appname, string taskname)
        {
            string endpoint = "/qrs/reloadtask/create";

            QlikSenseCreateTask task = new QlikSenseCreateTask();
            task.task = new QlikSenseTask();
            task.task.name = taskname;
            task.task.taskType = 0;
            task.task.enabled = true;
            task.task.taskSessionTimeout = 1440;
            task.task.maxRetries = 0;
            task.task.tags = new List<object>();
            
            task.task.app = new QlikSenseTaskApp();
            task.task.app.id = appid;
            task.task.app.name = appname;
            task.task.isManuallyTriggered = true;
            task.task.customProperties = new List<object>();

            string newtaskjsonin = JsonConvert.SerializeObject(task);

            string newtaskjsonout = qrsClient.Post(endpoint, newtaskjsonin);
            QlikSenseCreateTaskResult result = (QlikSenseCreateTaskResult)JsonConvert.DeserializeObject<QlikSenseCreateTaskResult>(newtaskjsonout);
            
            return result.id;
        }

        /*private void PrependScript(IApp app, string script)
        {

            string mainscript = app.GetScript();
            script = "///$tab LoopAndReduce\r\n" + script + "\r\n\r\n" + mainscript;
            app.SetScript(script);
            //app2.DoSave();

        }*/

        /*public void SetLoopAndReduceInfo(string appid, string loopandreducefield, string loopandreducevalue)
        {
            IApp apphandle = GetAppHandle(appid);
            string script = "set vLoopAndReduceFieldName = '" + loopandreducefield + "';\r\nset vLoopAndReduceFieldValue = '" + loopandreducevalue + "';\r\n\r\n";
            PrependScript(apphandle, script);

        }*/

        /*public void SetLoopAndReduceInfoBinary(string appid, string binaryfile)
        {
            IApp apphandle = GetAppHandle(appid);
            //binary [c:\QTARCHLAB\QlikStorage\LoopAndReduce\MyQlikView11App - A.qvw];
            string script = "binary [" + binaryfile + "];\r\n\r\n";
            PrependScript(apphandle, script);
        }*/

        /*public List<string> GetFieldValues(string appid, string field)
        {
            IApp apphandle = GetAppHandle(appid);
            //var inlineDimension = new ListboxListObjectDimensionDef { FieldDefs = new[] { field } };

            NxInlineDimensionDef inline = new NxInlineDimensionDef { FieldDefs = new[] { field } };

            ListObjectDef lo = new ListObjectDef();

            lo.Def = inline;

            GenericObjectProperties prop = new GenericObjectProperties();
            prop.Add(lo);

            try
            {

                CreateGenericObjectResult obj = apphandle.CreateGenericObject(prop);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
//            apphandle.AddFieldFromExpression
            

            //List l = new ListObject();
            
            
            //apphandle.CreateGenericObject(prop);
            return null;
            

            
        }*/


        //string apps = qrsClient.Get("/qrs/about");
        //QlikSenseAboutObject about = (QlikSenseAboutObject)JsonConvert.DeserializeObject <QlikSenseAboutObject>(apps);
        //string s = about.buildDate;
       /* public List<string> GetFieldValues(string appid, string field)
        {
         
            /*Field fld = app2.GetField(field);
            fld.Clear();
            int numvalues = fld.GetCardinal();

            List<string> fields = new List<string>();
            fields.Add(field);
            
            CreateSessionObjectParam param = new CreateSessionObjectParam();
            param.qInfo.qId = "LoopField";
            param.qInfo.qType = "ListObject";
            param.qListObjectDef.qDef.qFieldDefs = fields;

            //DynamicJson json = new DynamicJson();
            //json. = JsonConvert.SerializeObject(param);

            //GenericObjectProperties prop = new GenericObjectProperties();
            //prop.Add();

            

            //GenericObject o = app2.CreateGenericSessionObject(prop);

                //GetListObjectData
            
           // List<string> fields = new List<string>();

            /*string script = app2.GetScript();
            script = "///$tab LoopAndReduceValues\r\nset vLoopAndReduceFieldName = 'Store';\r\nset vLoopAndReduceFieldValue = 'StoreName#1';\r\n\r\n" + script;
            app2.SetScript(script);
            //app2.DoSave();

            //app2.DoReload();    //switch to async!!!

           // return fields;
        }*/
    }
}
