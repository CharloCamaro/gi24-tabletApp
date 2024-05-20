using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class calibrationController : MonoBehaviour
{

    public GameObject colorPatch;
    public GameObject colorData;
    public GameObject positionGuide; // Reference target matching the position of a colour on the colour palette (used to assist calibration)

    public GameObject colorPalette;
    public GameObject whitePatch;


    private int currRow = 0;
    private int currCol = 0;

    // Start is called before the first frame update
    // Initializes calibration colour to white.
    void Start()
    {
        currRow = 1;
        currCol = 1;
    }

    // Changes the colour of the reference plane to the next colour of the colour palette.
    public void ChangeColor()
    {
        IncreaseIndices();
        Debug.Log("Next color: " + currRow + ", " + currCol) ;
        colorPatch.GetComponent<SpriteRenderer>().color = colorData.GetComponent<colorHolder>().RetrieveColor(currRow-1, currCol-1);
        
    }

    // Changes the colour of the reference plane to a specific colour (taken from the colour palette using row,col indices.
    public void ChangeColor(int row, int col)
    {
        // Debug code such that when col == 99, change reference plane to a pure white plane (used for measuring max brightness of display)
        if (col == 99)
        {
            if (whitePatch.activeSelf)
            {
                colorPatch.SetActive(true);
                whitePatch.SetActive(false);
            }
            else
            {
                colorPatch.SetActive(false);
                whitePatch.SetActive(true);
            }
        }
        else
        {
            positionGuide.SetActive(true);
            Debug.Log("Changing color to: " + row + ", " + col);
            currRow = row;
            currCol = col;
            colorPatch.GetComponent<SpriteRenderer>().color = colorData.GetComponent<colorHolder>().RetrieveColor(row - 1, col - 1);

            // If targeting greyscale column, move reference target to not interfere with colorimeter.
            if (col == 0)
            {
                positionGuide.transform.position = colorPalette.transform.GetChild(row + 2).transform.position + new Vector3(0f, -0.95f, -4f);
            }
            else
            {
                positionGuide.transform.position = colorPalette.transform.GetChild(col + 9 + (row - 1) * 20).transform.position + new Vector3(0, -0.95f, -4f);
            }
        }
    }

    // Increment the currRow, currCol indices to help cycle through all colour palette colours during calibration.
    void IncreaseIndices()
    {
        if (currCol == 20)
        {
            currCol = 1;
            if (currRow == 7)
            {
                currRow = 1;
            }
            else
            {
                currRow = currRow + 1;
            }
        }
        else
        {
            currCol = currCol + 1;
        }
}
}
