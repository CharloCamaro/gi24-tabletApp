using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class buttonSelection : MonoBehaviour, IPointerDownHandler
{
    public GameObject buttonManager;
    public int buttonQuadrant = 0; // Value given to the vis button corresponding to one of the four quadrants {1,2,3,4}

   // Once button is pressed, transmit button value to session.
    public void OnPointerDown(PointerEventData eventData)
    {
        buttonManager.GetComponent<buttonController>().QuadrantSelected(buttonQuadrant);
    }
}
