using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonController : MonoBehaviour
{
    public GameObject comm; // Server communication game object
    public GameObject trialManager;

   
    // Returns quadrant value when a button is pressed during the vis task.
    public void QuadrantSelected(int quadrant)
    {
        trialManager.GetComponent<trialController>().EndVisTask(quadrant);
    }
}
