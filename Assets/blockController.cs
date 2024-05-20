using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blockController : MonoBehaviour
{

    public UXF.Session session;

    public GameObject sessionManager;
    public GameObject trialManager;
    public GameObject comm; // Server communication game object

    private int nbTrials = 10; // Number of trials per block (overwritten upon loading UXF settings)
    private float pauseTime = 2; // Time between trials in seconds (overwritten upon loading UXF settings)


    private int trialCount = 0; // Number of trials completed in block

    // Start is called before the first frame update
    void Start()
    {
        trialCount = 0;
    }

    // Creates new UXF block
    public void CreateNewBlock()
    {
        session.CreateBlock(nbTrials);
    }

    // Hides "pose" target before launching new block
    public void BeginBlock()
    {
        trialCount = 0;

        trialManager.GetComponent<trialController>().PrepareNextTrial();
    }

    // Initializes block parameter values based on UXF settings
    public void UpdateParameterValues()
    {
        nbTrials = session.settings.GetInt("nb trials per block");
        pauseTime = session.settings.GetFloat("pause time");
    }

    // Increments trial counter until block is completed
    public void UpdateTrialCounter()
    {
        trialCount++;
        if (trialCount < nbTrials)
        {
            trialManager.GetComponent<trialController>().PrepareNextTrial();
        }
        else
        {
            Debug.Log("Block done!");
            comm.GetComponent<WebsocketWithServer>().SendBlockDoneMessage();
            EndBlock();
        }
    }

    // Increments block counter at the end of the block
    void EndBlock()
    {
        sessionManager.GetComponent<sessionController>().UpdateBlockCounter();
    }

    // Communicates with server to state device is ready to launch next block.
    public void PrepareNextBlock()
    {
        comm.GetComponent<WebsocketWithServer>().SendBlockReadyMessage();
    }
}
