using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
using UnityEngine.InputSystem;
using Stopwatch = System.Diagnostics.Stopwatch;


public class trialController : MonoBehaviour
{
    public GameObject buttons; // Game object containing buttons used in vis task.
    public GameObject visScreen; // Game object containing pre-response screen for vis task.
    public GameObject colorChart; // Game object containing colour palette

    public GameObject comm; // Used to communicate with server
    public UXF.Session session;

    // Used to listen to keypresses during vis task.
    private PlayerInput playerInput;
    public InputAction responseAction;

    // Used to track time taken in each part of the trials.
    private Stopwatch timer;

    // Contains time taken for each of the three steps of a trial.
    private long visTaskCheckpoint;
    private long visRespCheckpoint;
    private long colorTaskCheckpoint;

    private bool hasPressed = false; // false until participant has pressed a key during a trial. 

   
    // Start is called before the first frame update
    void Start()
    {
        buttons.SetActive(false);
        colorChart.SetActive(false);
        timer = new Stopwatch();

        playerInput = GetComponent<PlayerInput>();
        responseAction = playerInput.actions["Responding"];
        responseAction.performed += TriggerResponse;
    }


    // Listens to keypresses to activate vis selection interface
    private void TriggerResponse(InputAction.CallbackContext context)
    {
        if ((timer.IsRunning) && (hasPressed == false))
        {
            hasPressed = true;
            ActivateVisButtons();
        }
    }


    // Launches new trial
    public void BeginTrial()
    {
        StartVisTask();
        session.BeginNextTrial();
        hasPressed = false;
        timer.Start();
        comm.GetComponent<WebsocketWithServer>().SendTrialStartedMessage();
    }


    // Launches vis task
    public void StartVisTask()
    {
        visScreen.SetActive(true);
    }
    

    // Ends vis task
    public void EndVisTask(int quadrant)
    {
        buttons.SetActive(false);

        Debug.Log("Quadrant selected: " + quadrant);

        visRespCheckpoint = timer.ElapsedMilliseconds;
        session.CurrentTrial.result["visRespTime"] = visRespCheckpoint-visTaskCheckpoint;

        session.CurrentTrial.result["quadrant"] = quadrant;

        comm.GetComponent<WebsocketWithServer>().SendVisTaskDoneMessage(quadrant);

        Invoke("StartColorTask", 0.25f);
    }


    // Launches colour matching task of a trial
    public void StartColorTask()
    {
        colorChart.SetActive(true);
    }


    // Ends colour matching task
    public void EndColorTask(Vector2 colorBin)
    {
        StoreColorTime();
        StoreColorResponse(colorBin);

        comm.GetComponent<WebsocketWithServer>().SendColorTaskDoneMessage(colorBin);

        colorChart.SetActive(false);

        EndCurrentTrial();
    }

    // Activates vis task buttons
    public void ActivateVisButtons()
    {
        visTaskCheckpoint = timer.ElapsedMilliseconds;
        session.CurrentTrial.result["visTaskTime"] = visTaskCheckpoint;

        comm.GetComponent<WebsocketWithServer>().SendHideVisMessage();
        visScreen.SetActive(false);
        buttons.SetActive(true);
    }

    // Prepares the next trial (resets trial)
    public void PrepareNextTrial()
    {
        timer.Reset();
        comm.GetComponent<WebsocketWithServer>().SendTrialReadyMessage();

    }

    // End the current trial of the study
    private void EndCurrentTrial()
    {
        timer.Stop();
        session.EndCurrentTrial();
        comm.GetComponent<WebsocketWithServer>().SendTrialDoneMessage();
    }

    // Saves colour indices selected in colour matching trial
    private void StoreColorResponse(Vector2 colorBin)
    {
        session.CurrentTrial.result["colorRow"] = colorBin.x;
        session.CurrentTrial.result["colorCol"] = colorBin.y;

        Debug.Log("Color selected (row,col): " + (int)(colorBin.x) + "," + (int)(colorBin.y));
    }

    // Saves time taken to select colour in colour matching task.
    private void StoreColorTime()
    {
        colorTaskCheckpoint = timer.ElapsedMilliseconds;

        session.CurrentTrial.result["colorTaskTime"] = colorTaskCheckpoint - visRespCheckpoint;
    }
}
