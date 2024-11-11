using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlacedInteriorTypeSO : ScriptableObject
{
    public Transform prefabCabinet;
    public Transform placingVisual;
    public Sprite sprite;
    public Type InteriorType;
    public string cabinetName;

    public enum Type
    {
        Cabinet,
        Table,
    }
    public enum Dir
    {
        Down,
        Up,
    }
    
    public int GetRotationAngle(Dir dir)
    {
        switch (dir)
        {
            default:
            case Dir.Down: return 180;
            case Dir.Up: return 0;
        }
    }

    public Dir SetRotationDirection(bool isUp)
    {
        if (isUp)
        {
            return Dir.Up;
        }
        else
        {
            return Dir.Down;
        }
    }

    
}
