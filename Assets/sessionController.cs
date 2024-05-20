using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UXF;

public class sessionController : MonoBehaviour
{
    public UXF.Session session;

    public GameObject trialManager;
    public GameObject blockManager;

    public GameObject comm; // Used to communicate with server

    private int nbBlocks = 1; // Number of block per session. Overwritten when loading UXF parameters
    private int blockCount = 0; // block counter

    private UXF.Settings expSettings = new UXF.Settings();
    private Dictionary<string, object> dict = new Dictionary<string, object>(); // dictionary holding session settings

    private string appStartTime;

    // Start is called before the first frame update
    void Start()
    {
        appStartTime = DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss.fff");
        comm.GetComponent<WebsocketWithServer>().SendStartTimeAppMessage(appStartTime);
        blockCount = 0;
    }

    // Launches UXF session
    public void LaunchSession()
    {
        comm.GetComponent<WebsocketWithServer>().LockParameters();
        session.Begin(expSettings.GetString("session name"), expSettings.GetString("participant id"), settings: expSettings);

        comm.GetComponent<WebsocketWithServer>().SendStartTimeSessMessage(DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss.fff"));

        DistributeSettings();

        blockManager.GetComponent<blockController>().PrepareNextBlock();
    }

    // Ends UXF session
    public void EndSession()
    {
        session.End();
    }

    // Increments counter when a block has been finished (and prepares the next)
    public void UpdateBlockCounter()
    {
        blockCount++;
        if (blockCount < nbBlocks)
        {
            blockManager.GetComponent<blockController>().PrepareNextBlock();
        }
        else
        {
            Debug.Log("experiment done!");
            session.End();
        }
    }

    // Updates UXF session settings
    public void UpdateParameterValues()
    {
        nbBlocks = session.settings.GetInt("nb blocks");
    }

    // Re-formats settings received in JSON format and feeds them to UXF session.
    public void ReceiveSessionParameters(string[] paramArray)
    {
        blockCount = 0;

        dict = new Dictionary<string, object>();
        expSettings = new UXF.Settings();

        for (int i = 0; i < paramArray.Length / 2; i++)
        {
            if (String.Equals(paramArray[2 * i], "block order"))
            {
                break;
            }
            dict.Add(paramArray[2 * i], paramArray[(2 * i) + 1]);
        }

        comm.GetComponent<WebsocketWithServer>().SendCsvDataMessage(dict["csv trials"].ToString());

        expSettings.UpdateWithDict(dict);

        if (String.Equals(dict["session name"], "practice"))
        {
            comm.GetComponent<WebsocketWithServer>().SendPracticeReadyMessage();
        }
        else
        {
            comm.GetComponent<WebsocketWithServer>().SendSessionReadyMessage();
        }
    }

    // Distributes UXF settings to other controllers.
    void DistributeSettings()
    {
        comm.GetComponent<WebsocketWithServer>().SendStartTimeAppMessage(appStartTime);
        UpdateParameterValues();
        blockManager.GetComponent<blockController>().UpdateParameterValues();
    }
}
