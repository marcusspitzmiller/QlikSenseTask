using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using QlikSenseJSONObjects;

namespace QlikSenseHelper
{
    class QlikSenseLoopAndReduce
    {
        private QlikSenseJSONHelper qs;
        
        private string url;
        private string sourceappname;
        private string loopandreducefield;
        private string streamname;
        private string folderpath;
        private string basefilename;

        public QlikSenseLoopAndReduce(string url)
        {
            this.url = url;
            

            //qs = new QlikSenseJSONHelper("https://qtarchlabus04.qliktech.com");
        }

        /*public bool LoopAndReduce(string sourceappname, string loopandreducefield, string streamname)
        {
            this.sourceappname = sourceappname;
            this.loopandreducefield = loopandreducefield;
            this.streamname = streamname;

            string sourceappid = qs.GetAppID(sourceappname);                
            string streamid = qs.GetStreamID(streamname);

            //use this for now
            //List<string> values = new List<string>();
            List<string> values = qs.GetFieldValues(sourceappid, loopandreducefield);
            
            values.Add("A");
            values.Add("B");
            values.Add("C");

            for (int i = 0; i < values.Count(); i++)
            {
                string targetapp = sourceappname + "_" + values[i];

                //first see if this app exists and get its info
                QlikSenseApp target = qs.GetApp(targetapp);
                
                //now copy
                string newappid = qs.CopyApp(sourceappid, targetapp);

                //qs.SetLoopAndReduceInfo(newappid, loopandreducefield, values[i]);

                //reload
                qs.Reload(newappid);

                //publish
                qs.Publish(newappid, streamid);

                //possibly replace the existing app
                if (target != null) //app exists
                {
                    qs.Delete(target.id);
                }
            }
            return true;
        }*/

        public bool LoopAndReduceBinary(string sourceappname, string folderpath, string basefilename, string streamname)
        {
            this.sourceappname = sourceappname;
            this.folderpath = folderpath;
            this.basefilename = basefilename;
            this.streamname = streamname;

            string sourceappid = qs.GetAppID(sourceappname);
            string streamid = qs.GetStreamID(streamname);

            //use this for now
            //List<string> values = new List<string>();
            //List<string> values = qs.GetFieldValues(sourceappid, loopandreducefield);
            string [] files = Directory.GetFiles(folderpath, basefilename + "*");
            
            for (int i = 0; i < files.Count(); i++)
            {
                string targetapp = sourceappname + files[i].Replace(folderpath, "").Replace(basefilename, "").Replace("\\", "");

                //first see if this app exists and get its info
                QlikSenseApp target = qs.GetApp(targetapp);
                
                //now copy
                string newappid = qs.CopyApp(sourceappid, targetapp);

               // qs.SetLoopAndReduceInfoBinary(newappid, files[i]);

                //reload
                string taskid = qs.CreateTask(newappid, targetapp, "Reload for Loop and Reduce");
                qs.StartTask(taskid, true);
                //qs.Reload(newappid);
                //qs.Reload(newappid);

                
                //possibly replace the existing app
                if (target != null) //app exists
                {
                    
                    qs.Replace(newappid, streamid);
                    qs.Delete(target.id); //unpublished copy
                }
                else
                {
                    //publish
                    qs.Publish(newappid, streamid);
                }
            }
            return true;

        }
    }
}
