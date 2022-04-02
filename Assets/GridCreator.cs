using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GridCreator : MonoBehaviour
{
    public Vector3 gridSize;
    public Transform[,,] currentState;
    
    public GameObject deadObject;
    public GameObject aliveObject;
    public GameObject inputField;
    InputField inputFieldUI;

    private bool isWhite = true;
    private bool isNewCounts = false;

    private void Start()
    {
        inputFieldUI = inputField.GetComponent<InputField>();
    }

    void CreateGrid()
    {
        currentState = new Transform[(int)gridSize.x, (int)gridSize.y, (int)gridSize.z];
        
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    GameObject GO = Instantiate(deadObject, gameObject.transform);
                    GO.transform.position = new Vector3(x, y, z);
                    currentState[x, y, z] = GO.transform;
                }
            }
        }
        
        gameObject.transform.Translate(gridSize/2 * -1);
    }

    void CalculateNextStep_OldWay()
    {
        /*
            Bir canlı hücrenin, iki'den daha az canlı komşusu varsa "yalnızlık nedeniyle" ölür
            Bir canlı hücrenin, üç'ten daha fazla canlı komşusu varsa "kalabalıklaşma nedeniyle" ölür
            Bir canlı hücrenin, iki ya da üç canlı komşusu varsa değişmeden bir sonraki nesile kalır
            Bir ölü hücrenin tam olarak üç canlı komşusu varsa canlanır.
         */
        
        for (int x = 0; x < currentState.GetLength(0); x++)
        {
            for (int y = 0; y < currentState.GetLength(1); y++)
            {
                for (int z = 0; z < currentState.GetLength(2); z++)
                {
                    if (currentState[x, y, z].GetComponent<MeshRenderer>() != null) //alive
                    {
                        int neigbourCount = CountAliveNeighbours(x, y, z);
                        currentState[x, y, z].GetChild(0).GetComponent<TextMesh>().text = neigbourCount.ToString();

                        if (neigbourCount < 2 || neigbourCount > 3) //dies
                        {
                            GameObject GO = Instantiate(deadObject, gameObject.transform);
                            GO.transform.position = currentState[x, y, z].position;
                            
                            Destroy(currentState[x, y, z].gameObject);
                            
                            currentState[x, y, z] = GO.transform;
                        }
                    }
                    else //dead
                    {
                        int neigbourCount = CountAliveNeighbours(x, y, z);
                        
                        if (neigbourCount == 3)
                        {
                            //starts living
                            currentState[x, y, z] = Instantiate(aliveObject, currentState[x, y, z]).transform;
                        }
                    }
                }
            }
        }
    }

    void CalculateNextStep_NewWay()
    {
        /*
            Bir canlı hücrenin, iki'den daha az canlı komşusu varsa "yalnızlık nedeniyle" ölür
            Bir canlı hücrenin, üç'ten daha fazla canlı komşusu varsa "kalabalıklaşma nedeniyle" ölür
            Bir canlı hücrenin, iki ya da üç canlı komşusu varsa değişmeden bir sonraki nesile kalır
            Bir ölü hücrenin tam olarak üç canlı komşusu varsa canlanır.
         */
        
        for (int x = 0; x < currentState.GetLength(0); x++)
        {
            for (int y = 0; y < currentState.GetLength(1); y++)
            {
                for (int z = 0; z < currentState.GetLength(2); z++)
                {
                    if (currentState[x, y, z].GetComponent<MeshRenderer>() != null) //alive
                    {
                        int neigbourCount = CountAliveNeighbours(x, y, z);
                        currentState[x, y, z].GetChild(0).GetComponent<TextMesh>().text = neigbourCount.ToString();

                        if (neigbourCount < 2 * 3 || neigbourCount > 3 * 3) //dies
                        {
                            GameObject GO = Instantiate(deadObject, gameObject.transform);
                            GO.transform.position = currentState[x, y, z].position;
                            
                            Destroy(currentState[x, y, z].gameObject);
                            
                            currentState[x, y, z] = GO.transform;
                        }
                    }
                    else //dead
                    {
                        int neigbourCount = CountAliveNeighbours(x, y, z);
                        
                        if (neigbourCount == 3 * 3)
                        {
                            //starts living
                            currentState[x, y, z] = Instantiate(aliveObject, currentState[x, y, z]).transform;
                        }
                    }
                }
            }
        }
    }

    int CountAliveNeighbours(int xPosition, int yPosition, int zPosition)
    {
        int count = 0;
        
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    if (xPosition + x < 0 || yPosition + y < 0 || zPosition + z < 0) continue;
                    
                    if (xPosition + x > currentState.GetLength(0) - 1 
                        || yPosition + y > currentState.GetLength(1) - 1 
                        || zPosition + z > currentState.GetLength(2) - 1 ) continue;
                    
                    if (currentState[xPosition + x, yPosition + y, zPosition + z].GetComponent<MeshRenderer>() != null) //alive neighbour
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }
    
    void CreateRandomCubes(int count)
    {
        isWhite = !isWhite;
        
        for (int i = 0; i < count; i++)
        {
            int x = Random.Range(0, (int)gridSize.x);
            int y = Random.Range(0, (int)gridSize.y);
            int z = Random.Range(0, (int)gridSize.z);

            currentState[x, y, z] = Instantiate(aliveObject, currentState[x, y, z]).transform;
        }
    }

    public void GetNumberFromInputField(string text)
    {
        int parsedInt;
        if (!int.TryParse(text, out parsedInt))
        {
            return;
        }

        if (gridSize.x == 0)
        {
            gridSize.x = parsedInt;
            
            inputFieldUI.DeactivateInputField();
            inputFieldUI.text = "";
            inputFieldUI.placeholder.gameObject.GetComponent<Text>().text = "Enter the y size of the grid...";
            inputFieldUI.ForceLabelUpdate();
            inputFieldUI.ActivateInputField();
        }
        else if (gridSize.y == 0)
        {
            gridSize.y = parsedInt;
            
            inputFieldUI.DeactivateInputField();
            inputFieldUI.text = "";
            inputFieldUI.placeholder.gameObject.GetComponent<Text>().text = "Enter the z size of the grid...";
            inputFieldUI.ForceLabelUpdate();
            inputFieldUI.ActivateInputField();
        }
        else if (gridSize.z == 0)
        {
            gridSize.z = parsedInt;
            
            inputField.transform.parent.gameObject.SetActive(false);
            CreateGrid();
        }
    }

    public void GetCountType(bool isTrue)
    {
        isNewCounts = isTrue;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateRandomCubes(15);
        }
        
        if (Input.GetKey(KeyCode.N))
        {
            if (isNewCounts)
            {
                CalculateNextStep_NewWay();
            }
            else
            {
                CalculateNextStep_OldWay();
            }
        }
    }
}
