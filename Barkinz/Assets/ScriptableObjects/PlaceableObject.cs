using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Placeable", menuName ="Purchase Items/Placeable Object")]
public class PlaceableObject : ScriptableObject
{
    public string ObjectLookup;
    public Mesh ObjectMesh;
    public Texture2D ObjectTexture;
    public float Width, Length;
}

