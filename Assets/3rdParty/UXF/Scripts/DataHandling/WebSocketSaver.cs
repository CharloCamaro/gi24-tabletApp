using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Globalization;

namespace UXF
{
    /// <summary>
    /// Component which manages File I/O in a seperate thread to avoid hitches.
    /// </summary>
    public class WebSocketSaver : DataHandler
    {
        public GameObject comm;

        /// <summary>
        /// Enable to force the data to save with an english-US format (i.e. `,` to serapate values,
        /// and `.` to separate decimal points).
        /// </summary>
        [Tooltip("Enable to force the data to save with an english-US format (i.e. `,` to serapate values, and `.` to separate decimal points).")]
        public bool forceENUSLocale = true;

        /// <summary>
        /// Enable to print debug messages to the console.
        /// </summary>
        [Tooltip("Enable to print debug messages to the console.")]
        public bool verboseDebug = false;

        /// <summary>
        /// An action which does nothing.
        /// </summary>
        /// <returns></returns>
        public static System.Action doNothing = () => { };

        public bool IsActive { get { return parallelThread != null && parallelThread.IsAlive; } }


        BlockingQueue<System.Action> bq = new BlockingQueue<System.Action>();
        Thread parallelThread;

        bool quitting = false;


        /// <summary>
        /// Starts the FileSaver Worker thread.
        /// </summary>
        public override void SetUp()
        {
            if (forceENUSLocale)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            }

            quitting = false;

            if (!IsActive)
            {
                parallelThread = new Thread(Worker);
                if (forceENUSLocale)
                {
                    parallelThread.CurrentCulture = new CultureInfo("en-US");
                }
                parallelThread.Start();
            }
            else
            {
                Utilities.UXFDebugLogWarning("Parallel thread is still active!");
            }
        }

        /// <summary>
        /// Adds a new command to a queue which is executed in a separate worker thread when it is available.
        /// Warning: The Unity Engine API is not thread safe, so do not attempt to put any Unity commands here.
        /// </summary>
        /// <param name="action"></param>
        public void ManageInWorker(System.Action action)
        {

            if (quitting)
            {
                throw new System.InvalidOperationException(
                    string.Format(
                        "Cannot add action to FileSaver, is currently quitting. Action: {0}.{1}",
                        action.Method.ReflectedType.FullName,
                        action.Method.Name
                        )
                );
            }

            bq.Enqueue(action);
        }

        void Worker()
        {
            if (verboseDebug)
                Utilities.UXFDebugLog("Started worker thread");

            // performs FileIO tasks in seperate thread
            foreach (var action in bq)
            {
                if (verboseDebug)
                    Utilities.UXFDebugLogFormat("Managing action");

                try
                {
                    action.Invoke();                
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                /*catch (IOException e)
                {
                    Utilities.UXFDebugLogError(string.Format("Error, file may be in use! Exception: {0}", e));
                }*/
                catch (System.Exception e)
                {
                    // stops thread aborting upon an exception
                    Debug.LogException(e);
                }

                if (quitting && bq.NumItems() == 0)
                    break;
            }

            if (verboseDebug)
                Utilities.UXFDebugLog("Finished worker thread");
        }

        /// <summary>
        /// Returns true if there may be a risk of overwriting data.
        /// </summary>
        /// <param name="experiment"></param>
        /// <param name="ppid"></param>
        /// <param name="sessionNum"></param>
        /// <returns></returns>
        public override bool CheckIfRiskOfOverwrite(string experiment, string ppid, int sessionNum, string rootPath = "")
        {
            return false;
        }


        public override string HandleDataTable(UXFDataTable table, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {

            string[] lines = table.GetCSVLines();

            if (dataType == UXFDataType.TrialResults)
            {
                string resultString = string.Join("\n", lines);

                comm.GetComponent<WebsocketWithServer>().SendCsvResultsMessage(resultString);
            }

            return "?";
        }

        public override string HandleJSONSerializableObject(List<object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            return "?";
        }

        public override string HandleJSONSerializableObject(Dictionary<string, object> serializableObject, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            return "?";
        }
        
        public override string HandleText(string text, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            return "?";
        }

        public override string HandleBytes(byte[] bytes, string experiment, string ppid, int sessionNum, string dataName, UXFDataType dataType, int optionalTrialNum = 0)
        {
            return "?";
        }
       
        public static string SessionNumToName(int num)
        {
            return string.Format("S{0:000}", num);
        }

        /// <summary>
        /// Aborts the FileSaver's thread and joins the thread to the calling thread.
        /// </summary>
        public override void CleanUp()
        {
            if (verboseDebug)
                Utilities.UXFDebugLog("Joining File Saver Thread");
            quitting = true;
            bq.Enqueue(doNothing); // ensures bq breaks from foreach loop
            parallelThread.Join();
        }

    }
}
