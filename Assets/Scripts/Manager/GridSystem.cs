using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCTCodeBase.Base;

public class GridSystem<TGridObject>
{
    /*public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;

    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int z;
    }*/

    public const int sortingOrderDefault = 5000;

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;
   

    public GridSystem(int width, int height, float cellSize, Vector3 originPosition, Func<GridSystem<TGridObject>, int, int , TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int i = 0; i < gridArray.GetLength(0); i++)
        {
            for (int j = 0; j < gridArray.GetLength(1); j++)
            {
                gridArray[i, j] = createGridObject(this, i, j);
            }
        }

        bool showDebug = true;

        if (showDebug)
        {
            TextMesh[,] debugTextArray = new TextMesh[width, height];
            for (int i = 0; i < gridArray.GetLength(0); i++)
            {
                for (int j = 0; j < gridArray.GetLength(1); j++)
                {
                    //debugTextArray[i,j] = CodeBaseClass.CreateWorldText(gridArray[i, j]?.ToString(), null, GetWorldPosition(i, j) + new Vector3(cellSize, 0, cellSize) * 0.5f, 15, Color.white, TextAnchor.MiddleCenter, TextAlignment.Center);
                    Debug.DrawLine(GetWorldPosition(i, j), GetWorldPosition(i, j + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(i, j), GetWorldPosition(i + 1, j), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

            // Update the DebugLine 
            /*OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) =>
            {
                //debugTextArray[eventArgs.x, eventArgs.z].text = gridArray[eventArgs.x, eventArgs.z]?.ToString();
            };*/
        }
        
    }

    public Vector2Int ValidateGridPosition(Vector2Int gridPosition)
    {
        // Round up
        return new Vector2Int(
            Mathf.Clamp(gridPosition.x, 0, width - 1),
            Mathf.Clamp(gridPosition.y, 0, height - 1)
        );
    }

    /*public void TriggerGridCabinetChanged(int x, int z)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, z = z });
    }*/


    public Vector3 GetWorldPosition(int x, int z)
    {
        //Debug.Log("GetWorldPosition: " + new Vector3(x, 0, z) * cellSize + originPosition);
        return new Vector3(x, 0, z) * cellSize + originPosition;
    }

    public Vector3 GetPlacedCabinetFloorGridPosition(Vector3 floorPosition, PlacedInteriorTypeSO cabinetSO)
    {
        if (floorPosition.z >= -cellSize)
        {
            return floorPosition + new Vector3(cellSize * 0.5f, 0, cellSize * 0.75f);

        }
        if (floorPosition.z <= -cellSize * 2) // Is down
        {
            return floorPosition + new Vector3(cellSize * 0.5f, 0, cellSize * 0.25f);
        }
        else
        {
            Debug.LogError("This should never reach!!");
            Debug.Log(floorPosition.z);
            return floorPosition + new Vector3(cellSize, 0, cellSize) * 0.5f;
        }
    }

    public bool IsFacingUp(Vector3 floorPosition)
    {
        return floorPosition.z <= -cellSize * 2;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public void GetCellXZByPosition(Vector3 worldPosition, Vector3 floorPosition , out int x, out int z)
    {
        // Round down the float and divide to cellSize to represent the Cell base on xz
        Vector3 cellPosition = worldPosition - floorPosition;
        x = Mathf.FloorToInt(cellPosition.x / cellSize);
        z = Mathf.FloorToInt(cellPosition.z / cellSize);
    }

    public TGridObject GetGridObject(int x, int z)
    {
        if (x >= 0 && z >= 0 && x < width && z < height)
        {
            return gridArray[x, z];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 worldPosition, Vector3 floorPosition)
    {
        int x, z;
        GetCellXZByPosition(worldPosition, floorPosition, out x, out z);
        return GetGridObject(x, z);
    }

    /*public void TriggerGridObjectChanged(int x, int z)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, z = z });
    }*/

}
