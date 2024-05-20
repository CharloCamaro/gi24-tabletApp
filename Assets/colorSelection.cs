using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class colorSelection : MonoBehaviour, IPointerDownHandler
{
    public int rowBin = 0; // stores row index of colour in colour palette
    public int colBin = 0; // stores col index of colour in colour palette
    public GameObject trialManager;


   // On pressing colour palette button, end colour matching task
    public void OnPointerDown(PointerEventData eventData)
    {
        trialManager.GetComponent<trialController>().EndColorTask(new Vector2((float)rowBin,(float)colBin));
    }

    // Initialize parameter when colour button is initialized.
    public void UpdateValues(GameObject trialMan, int row, int col)
    {
        trialManager = trialMan;
        rowBin = row;
        colBin = col;
    }
}
