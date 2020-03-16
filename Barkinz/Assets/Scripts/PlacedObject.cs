﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PlacedObject : MonoBehaviour
{
    public PlaceableObject ObjectInformation;
    public Vector2 GridPosition;
    MeshRenderer objectRenderer { get => GetComponent<MeshRenderer>(); }
    MeshFilter objectFilter { get => GetComponent<MeshFilter>(); }

    public void InitializePlacedObject()
    {
        InitializePlacedObject(Vector2.zero);
    }

    public void InitializePlacedObject(Vector2 gridPosition)
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
}
