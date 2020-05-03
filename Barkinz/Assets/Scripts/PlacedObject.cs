using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PlacedObject : MonoBehaviour
{
    public PlaceableObject ObjectInformation;
    public Vector2Int GridPosition;
    protected Tile PlacedOn;
    Renderer objectRenderer { get => GetComponent<Renderer>(); }
    MeshFilter objectFilter { get => GetComponent<MeshFilter>(); }

    public void InitializePlacedObject()
    {
        InitializePlacedObject(Vector2Int.zero);
    }

    public void InitializePlacedObject(Vector2Int gridPosition)
    {
        if (ObjectInformation != null)
        {
            if (ObjectInformation.ObjectTexture != null)
            {
                objectRenderer.material.mainTexture = ObjectInformation.ObjectTexture;
            }
            if (ObjectInformation.ObjectMesh != null) 
            {
                objectFilter.mesh = ObjectInformation.ObjectMesh;
            }
        }
        GridPosition = gridPosition;
    }

    public void RotateObject()
    {
        Vector3 rotationEulers = transform.eulerAngles + new Vector3(0, 90, 0);
        transform.rotation = Quaternion.Euler(rotationEulers);
    }

    public void InitializePlacedObject(Vector2Int gridPosition, PlaceableObject obj)
    {
        ObjectInformation = obj;
        InitializePlacedObject(gridPosition);
    }
}
