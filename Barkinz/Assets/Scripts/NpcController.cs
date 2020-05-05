using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, IZoomOn
{
    public Tile TileAt;
    public SpriteRenderer OverworldSpriteDisplay;
    public Transform ZoomObjectTransform { get => transform; }
    public float CameraOrthoSize => 2;
    public Vector3 ZoomCamPosition()
    {
        Vector3 p = transform.position + new Vector3(0, 1, -1.5f);
        return p;
    }

    public void SetBarkinz(BarkinzInfo b, Tile t)
    {
        OverworldSpriteDisplay.sprite = b.MainBarkinzSprite;
        transform.position = t.centerPosition;
    }
}
