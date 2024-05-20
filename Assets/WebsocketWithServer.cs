using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using NativeWebSocket;

public class WebsocketWithServer : MonoBehaviour
{
    public int portNumber = 3100; // Port number to be used to communicate with server
    public string clientHeader = "TB-"; // header of every message sent by the device. Server will only respond to specific headers.

    public GameObject expBuilder;
    public GameObject calibrationManager;

    public GameObject sessionManager;
    public GameObject blockManager;
    public GameObject trialManager;

    public GameObject calibScreen;
    public GameObject palette;

    public bool tabletBuild = false;  // Set to "true" when deploying app to device rather than testing it inside Unity.

    private WebSocket websocket;

    private bool parametersReceived = false; // States whether parameter settings have been received by the device and if they can be modified again.

    // Start is called before the first frame update
    async void Start()
    {
        parametersReceived = false;

        // Deployments only currently work when the server and all devices are on the same network (e.g., hotspot).
        if (tabletBuild)
        {
            websocket = new WebSocket("ws://192.168.137.1:" + portNumber.ToString());
        }
        else
        {
            websocket = new WebSocket("ws://localhost:"+portNumber.ToString());
        }
        
        websocket.OnOpen += () =>
        {
            SendMessageToServer("Connected");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // getting the message as a string

            var message = System.Text.Encoding.UTF8.GetString(bytes);
            var header = message.Substring(0,2).ToUpper();

            // If request is valid, process it.
            if (ValidateHeader(header))
            {
                Debug.Log("Client ID: " + header + "; " + message.Remove(0, 3));

                ReadMessage(message.Remove(0, 3));
            }
        };

        // waiting for messages
        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    // Communicates request to hide visualization (after participants presses a key)
    public void SendHideVisMessage()
    {
        SendMessageToServer("HideVis");
    }

    // Comunicates vis task is finished, and sends response
    public void SendVisTaskDoneMessage(int quadrant)
    {
        SendMessageToServer("VisTaskDone" + " " + quadrant);
    }

    // Communicates colour matching task is finished, and sends response
    public void SendColorTaskDoneMessage(Vector2 colorBin)
    {
        SendMessageToServer("ColTaskDone" + " " + (int)colorBin.x + "," + (int)colorBin.y);
    }

    // Communicates session is ready
    public void SendSessionReadyMessage()
    {
        SendMessageToServer("SessionReady");
    }

    // Communicates block is finished.
    public void SendBlockDoneMessage()
    {
        SendMessageToServer("BlockDone");
    }

    // Communicates block is ready to begin
    public void SendBlockReadyMessage()
    {
        SendMessageToServer("BlockReady");
    }

    // Communicates a new trial has begun
    public void SendTrialStartedMessage()
    {
        SendMessageToServer("TrialStarted");
    }

    // Communicates trial is ready to begin
    public void SendTrialReadyMessage()
    {
        SendMessageToServer("TrialReady");
    }

    // Communicates trial is finished.
    public void SendTrialDoneMessage()
    {
        SendMessageToServer("TrialDone");       
    }

    // Communicates session is finished
    public void SendSessionDoneMessage()
    {
        SendMessageToServer("SessionDone");
    }

    // Communicates practice session is ready to begin
    public void SendPracticeReadyMessage()
    {
        SendMessageToServer("PracticeReady");
    }

    // Communicates start time of UXF session
    public void SendStartTimeSessMessage(string timestamp)
    {
        SendMessageToServer("StartTimeSess " + timestamp);
    }

    // Communicates start time of application
    public void SendStartTimeAppMessage(string timestamp)
    {
        SendMessageToServer("StartTimeApp " + timestamp);
    }

    // Communicates custom messages
    public void SendMessageToServer(string message)
    {
        if (websocket.State == WebSocketState.Open)
        {
            websocket.SendText(clientHeader+message);
        }
    }

    // Requests CSV data
    public void SendCsvDataMessage(string filename)
    {
        SendMessageToServer("ProvideCsvData " + filename);
    }

    // Communicates session results in CSV format.
    public void SendCsvResultsMessage(string data)
    {
        Debug.Log("Sending result: " + data);
        SendMessageToServer("ReceiveResults " + data);
    }

    // Communicates information about the session block
    public void SendBlockInfoMessage(string blockInfo)
    {
        SendMessageToServer("BlockInfo " + blockInfo);
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    private void ReadMessage(string message)
    {
        message = message.ToUpper();

        // Responds to start-trial request
        if (String.Equals(message, "TRIAL_BEGIN"))
        {
            trialManager.GetComponent<trialController>().BeginTrial();
        }
        // Responds to session-launch request
        else if (String.Equals(message, "SESS_BEGIN"))
        {
            sessionManager.GetComponent<sessionController>().LaunchSession();

            SendMessageToServer("SESS_STARTED");
        }
        // Responds to session-end request
        else if (String.Equals(message, "SESS_END"))
        {
            sessionManager.GetComponent<sessionController>().EndSession();
            Debug.Log("Session ended!");
        }
        // Responds to reception of CSV data
        else if (String.Equals(message.Substring(0, 1), "{"))
        {
            if (!parametersReceived)
            {
                Debug.Log("Parameters received!");

                var trimmedMessage = message.Remove(0, 1);
                trimmedMessage = trimmedMessage.Remove(trimmedMessage.Length - 1, 1).ToLower();
                char[] delimiterChars = { ':', ',', '\t', '\n' };
                string[] paramArray = trimmedMessage.Split(delimiterChars);

                char[] badChars = { '"' };
                for (int i = 0; i < paramArray.Length; i++)
                {
                    paramArray[i] = paramArray[i].Trim(badChars);
                }

                sessionManager.GetComponent<sessionController>().ReceiveSessionParameters(paramArray);
            }

        }
        // Responds to setup-practice-session request
        else if (String.Equals(message, "SETUP_REQUEST_P"))
        {
            if (!parametersReceived)
            {
                SendMessageToServer("ProvideParametersP");
            }
            else
            {
                SendPracticeReadyMessage();
            }
        }
        // Responds to setup-study-session request
        else if (String.Equals(message, "SETUP_REQUEST_S"))
        {
            if (!parametersReceived)
            {
                SendMessageToServer("ProvideParametersS");
            }
            else
            {
                SendSessionReadyMessage();
            }
        }
        // Responds to start-block request
        else if (String.Equals(message, "BLOCK_BEGIN"))
        {
            blockManager.GetComponent<blockController>().BeginBlock();
        }
        // Responds to request to change colour in calibration
        else if (String.Equals(message.Substring(0, 5), "COLOR"))
        {
            Debug.Log("Calibration step detected");
            if (String.Equals(message, "COLOR"))
            {
                calibrationManager.GetComponent<calibrationController>().ChangeColor();
            }
            else
            {
                var indices = message.Remove(0, 5);
                Debug.Log(indices);
                int row = Int32.Parse(indices.Substring(0, 2));
                int col = Int32.Parse(indices.Substring(3, 2));
                Debug.Log(row + "-" + col);
                calibrationManager.GetComponent<calibrationController>().ChangeColor(row, col);
            }
        }
        // Responds to create-experiment request
        else if (String.Equals(message.Substring(0, 8), "CSV_DATA"))
        {
            var trimmedMessage = message.Remove(0, 9).ToLower();
            var fixedTrimmedMessage = trimmedMessage.Replace("\r", "");
            char[] delimiterChars = { '\n' };
            string[] trialSetupData = fixedTrimmedMessage.Split(delimiterChars);

            expBuilder.GetComponent<CSVExperimentBuilder>().csvData = trialSetupData.Take(trialSetupData.Count() - 1).ToArray();
        }
        // Responds to begin-calibration request
        else if (String.Equals(message, "CALIB_BEGIN"))
        {
            calibScreen.SetActive(true);
        }
        // Responds to end-calibration request
        else if (String.Equals(message, "CALIB_END"))
        {
            calibScreen.SetActive(false);
        }
        // Responds to show-colour-palette request
        else if (String.Equals(message, "SHOW_PALETTE"))
        {
            palette.SetActive(!palette.activeSelf);
        }
    }

    // Validates request was sent by server.
    private bool ValidateHeader(string header)
    {
        return String.Equals(header, "SV");
    }

    // Locks parameters to make sure they cannot be modified again during a session
    public void LockParameters()
    {
        parametersReceived = true;
    }

    // Unlocks parameters to allow them to be modified outside a session
    public void UnlockParameters()
    {
        parametersReceived = false;
    }
}