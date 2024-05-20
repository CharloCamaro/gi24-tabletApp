using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class colorGridBuilder : MonoBehaviour
{
    public GameObject trialManager;

    public GameObject colorData; // Game object containing a list of RGB colours that define the colour palette

    public int nRowColors = 2; // Number of rows in the colour palette
    public int nColColors = 4; // Number of cols in the colour palette
    public int nGrey = 8;  // Number of greys in the greyscale column

    // Values for a cube gameobject to cover the full camera FoV (1080p).
    public float xTransformScale = 18.1761f;
    public float yTransformScale = 10f;

    // Spacing between colours on the colour palette
    public float xSpacing = 0.1f;
    public float ySpacing = 0.1f;

    // Achromatic space around the palette
    public float xBuffer = 0.02f;
    public float yBuffer = 0.05f;

    // Prefabs for the elements of the colour palette.
    public GameObject buttonPrefab;
    public GameObject numberPrefab;

    public GameObject colorChartGrid;

    private GameObject textHolder; // Game object to store all the text elements

    private float xOffset = 0f; 
    private float yOffset = 0f;

    private GameObject[,] grid; // List of chromatic buttons part of the colour palette
    private GameObject[] gridBW; // List of greyscale buttons part of the colour palette

    private string[] colNames = new string[26] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

    // This script automatically generates the appearance of the colour palette based on the number of rows/columns.
    void Start()
    {
        gameObject.GetComponent<colorHolder>().PrepareColors();

        textHolder = new GameObject("Text holder");
        textHolder.transform.parent = colorChartGrid.transform;

        grid = new GameObject[nRowColors, nColColors];
        gridBW = new GameObject[nGrey];

        var pctCellWidth = 1 / (float)nColColors;

        var xFreeRatio = 1 - (2 * xBuffer * (3 / 5));
        var yFreeRatio = 1 - (2 * yBuffer);


        // Determine usable space to distribute buttons
        var xLength = xTransformScale * xFreeRatio;
        var yLength = yTransformScale * yFreeRatio;

        // Divide by number of elements to be shown to obtain button+spacing size
        var xButtonSize = (xLength / (nColColors + 1 + 1)) - xSpacing;
        var yButtonSize = (yLength / (nRowColors + 1)) - ySpacing;


        xOffset = colorChartGrid.transform.position.x;
        yOffset = colorChartGrid.transform.position.y;

        var xScale = xTransformScale / (float)nColColors;
        var yScale = yTransformScale / (float)nRowColors;

        float xPosition = 0f;
        float yPosition = 0f;

        // Create letter column
        xPosition = xOffset + (1.25f * xButtonSize) + (xButtonSize / 2) + (xBuffer * (3 / 5) * xTransformScale);
        GameObject currTextObject;
        for (int row = 0; row <= nRowColors; row++)
        {
            yPosition = yOffset - ((row-1) * ySpacing) - (yButtonSize * row) - (yButtonSize*0) - (yButtonSize / 2) - (yBuffer * yTransformScale);
            currTextObject = Instantiate(numberPrefab, new Vector2(xPosition, yPosition), Quaternion.identity, textHolder.transform);
            currTextObject.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = colNames[row];
        }
        // Create greyscal UI elements

        xPosition = xOffset + (0.25f * xButtonSize) + (xButtonSize / 2) + (xBuffer * (3 / 5) * xTransformScale);
        for (int row = 0; row < nGrey;row++)
        {
            
            yPosition = yOffset - (row * ySpacing) - (yButtonSize * row) - (yButtonSize / 2) + (ySpacing) - (yBuffer * yTransformScale);

            gridBW[row] = Instantiate(buttonPrefab, new Vector2(xPosition, yPosition), Quaternion.identity, colorChartGrid.transform);

            gridBW[row].transform.localScale = new Vector2(xButtonSize, yButtonSize);

            gridBW[row].GetComponent<colorSelection>().UpdateValues(trialManager, row-1, -1);
            var randColor = UnityEngine.Random.Range(0f, 1f);
            gridBW[row].GetComponent<SpriteRenderer>().color = colorData.GetComponent<colorHolder>().RetrieveColor(row - 1, -1);
        }

        // Create number row
        yPosition = yOffset - (0 * ySpacing) - (yButtonSize * 0) - (0 * yButtonSize) - (yButtonSize / 2) - (yBuffer * yTransformScale);

        for (int col = 0; col < nColColors;col++)
        {
            xPosition = (col * xSpacing) + (xButtonSize * col) + xOffset + (2 * xButtonSize) + (xButtonSize / 2) + (xButtonSize / 4) + (xBuffer * (3 / 5) * xTransformScale);

            currTextObject = Instantiate(numberPrefab, new Vector2(xPosition, yPosition), Quaternion.identity, textHolder.transform);
            currTextObject.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (col+1).ToString();
        }

        // Create color UI elements
        for (int row = 0; row < nRowColors; row++)
        {
            yPosition = (yScale * row) + yOffset + (yScale / 2);

            for (int col = 0; col < nColColors;col++)
            {

                xPosition = (col*xSpacing) + (xButtonSize * col) + xOffset + (2*xButtonSize) + (xButtonSize / 2) + (xButtonSize/4) + (xBuffer*(3/5)*xTransformScale);
                yPosition = yOffset - (row * ySpacing) - (yButtonSize * row) - (1 * yButtonSize) - (yButtonSize / 2) - (yBuffer * yTransformScale);

                grid[row,col] = Instantiate(buttonPrefab, new Vector2(xPosition, yPosition), Quaternion.identity, colorChartGrid.transform);

                grid[row, col].transform.localScale = new Vector2(xButtonSize, yButtonSize);

                grid[row, col].GetComponent<colorSelection>().UpdateValues(trialManager, row, col);

                grid[row,col].GetComponent<SpriteRenderer>().color = gameObject.GetComponent<colorHolder>().RetrieveColor(row,col);
            }
        }

        // Hide colour palette until  necessary.
        colorChartGrid.SetActive(false);
    }

}
