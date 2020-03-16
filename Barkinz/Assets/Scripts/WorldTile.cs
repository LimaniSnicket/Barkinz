﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
public class WorldTile : MonoBehaviour
{
    public int tileRows, tileColumns;
    public List<Tile> Tiles;
    MeshRenderer thisMesh { get => GetComponent<MeshRenderer>(); }
    public Sprite TileSprite;
    public Dictionary<GameObject, Tile> TileLookup;
    public Tile[,] GridPositions;

    public Tile PlayerPositionTile;

    public GameObject PlaceableObjectPrefab;
    public PlaceableObject TestPlacements;

    private void Awake()
    {
        BarkinzManager.InitializeBarkinzData += OnBarkinzLoad;
        PlayerPositionTile = new Tile();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetTargetTileViaClick();
        }

        if (MinigameManager.ValidMode(ActiveGameFunction.NONE))
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                PlayerPositionTile = GetAdjacentTile(Vector2.right);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                PlayerPositionTile = GetAdjacentTile(Vector2.right * -1);
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                PlayerPositionTile = GetAdjacentTile(Vector2.up * -1);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                PlayerPositionTile = GetAdjacentTile(Vector2.up);
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                PlacedObject p = Instantiate(PlaceableObjectPrefab).GetComponent<PlacedObject>();
                p.InitializePlacedObject(new Vector2(0, 0));
                Tile placedAt = GetTileAtPosition((int)p.GridPosition.x, (int)p.GridPosition.y);
                placedAt.SetPlacedObjectPosition(p);
            }

        }
    }

    public Tile GetAdjacentTile(Vector2 gridOffset)
    {
        try
        {
            Vector2 o = PlayerPositionTile.GridPosition + gridOffset;
            Tile t = GridPositions[(int)o.x, (int)o.y];
            if (!t.occupied)
            {
                QueueTile(t);
                return GridPositions[(int)o.x, (int)o.y];
            } else
            {
                return PlayerPositionTile;
            }
        }
        catch (IndexOutOfRangeException)
        {
            return PlayerPositionTile;
        }
    }

    public Tile GetTileAtPosition(int row, int column)
    {
        try
        {
            return GridPositions[row, column];
        }
        catch (IndexOutOfRangeException) { return GridPositions[0, 0]; }
    }

    public static event Action<Tile> TileSelected;
    public static event Action<Tile> QueueTile;

    public void SetTargetTileViaClick()
    {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rh = new RaycastHit();
        if (Physics.SphereCast(r, 1, out rh, Mathf.Infinity) && TileLookup.ContainsKey(rh.transform.gameObject))
        {
            if (!TileLookup[rh.transform.gameObject].occupied)
            {
                TileLookup[rh.transform.gameObject].occupied = true;
                TileSelected(TileLookup[rh.transform.gameObject]);
            }
        }
    }

    void OnBarkinzLoad(BarkinzInfo b)
    {
        TileLookup = new Dictionary<GameObject, Tile>();
        Tiles = new List<Tile>();
        GridPositions = new Tile[tileRows, tileColumns];
        if (!b.LoadSettingsFromInfo)
        {
            GenerateDefaultTileMap();
            QueueTile(PlayerPositionTile);
        } else
        {
            b.SetWorldTileFromSettings(this);
        }
    }

    public void GenerateDefaultTileMap()
    {
        if (tileColumns == 0) { tileColumns = 1; }
        if (tileRows == 0) { tileRows = 1; }
        float tileWidth = thisMesh.bounds.size.x / tileColumns;
        float tileLength = thisMesh.bounds.size.z / tileRows;
        Vector3 spawnPoint = new Vector3(-thisMesh.bounds.extents.x + tileWidth / 2, transform.position.y, thisMesh.bounds.extents.z - tileLength / 2);
        Debug.Log(spawnPoint);
        for (int i = 0; i < tileRows; i++)
        {
            for (int j = 0; j < tileColumns; j++)
            {
                Tile t = new Tile(tileWidth, tileLength, spawnPoint + new Vector3(tileWidth * i, 0, -tileLength * j), i, j);
                t.InitializeTile(TileSprite, this);
                Tiles.Add(t);
                TileLookup.Add(t.GetGameObject(), t);
                GridPositions[i, j] = t;
            }
        }
        PlayerPositionTile = GridPositions[0,0];
    }

    void UpdatePrimaryBarkinzOnQuit(BarkinzInfo b)
    {
        b.UpdateWorldTileSettings(this);
    }

    private void OnDestroy()
    {
        BarkinzManager.InitializeBarkinzData -= OnBarkinzLoad;
    }

    private void OnApplicationQuit()
    {
        UpdatePrimaryBarkinzOnQuit(BarkinzManager.PrimaryBarkinz);
    }
}

[Serializable]
public class Tile
{
    private GameObject go;
    private SpriteRenderer TileRenderer;
    public float width, length;
    public bool occupied;
    public Vector3 centerPosition;
    public Vector2 GridPosition { get; private set; }
    public Tile() { width = 1; length = 1; occupied = false; centerPosition = Vector3.zero; }
    public Tile(Vector3 cp) { width = 1; length = 1; occupied = false; centerPosition = cp; }
    public Tile(float w, float l, Vector3 cp) { width = w; length = l; centerPosition = cp; occupied = false; GridPosition = new Vector2(0, 0); }
    public Tile(float w, float l, Vector3 cp, int row, int column) { width = w; length = l; centerPosition = cp; occupied = false; GridPosition = new Vector2(row, column); }
    public void InitializeTile(Sprite s, WorldTile parentTile)
    {
        GameObject g = new GameObject("Tile Sprite");
        go = g;
        g.transform.position = centerPosition;
        g.transform.forward = Vector3.up;
        TileRenderer = g.AddComponent<SpriteRenderer>();
        BoxCollider b = g.AddComponent<BoxCollider>();
        b.isTrigger = true;
        TileRenderer.sprite = s;
        TileRenderer.color = Color.red;
        g.transform.SetParent(parentTile.transform);
    }

    public void InitializeTile(Sprite s, WorldTile parentTile, int i, int j)
    {
        GameObject g = new GameObject("Tile Sprite");
        go = g;
        g.transform.position = centerPosition;
        g.transform.forward = Vector3.up;
        TileRenderer = g.AddComponent<SpriteRenderer>();
        BoxCollider b = g.AddComponent<BoxCollider>();
        b.isTrigger = true;
        TileRenderer.sprite = s;
        TileRenderer.color = Color.red;
        g.transform.SetParent(parentTile.transform);
        GridPosition = new Vector2(i, j);
    }

    public GameObject GetGameObject()
    {
        return go;
    }

    public void SetPlacedObjectPosition(PlacedObject p)
    {
        p.transform.position = centerPosition;
        occupied = true;
    }
}

[Serializable]
public class WorldTileSettings
{
    public List<Tile> TileLayoutSettings;
    public Tile PlayerTile;
    public WorldTileSettings() { TileLayoutSettings = new List<Tile>(); }
    public WorldTileSettings(WorldTile toSave)
    {
        TileLayoutSettings = new List<Tile>(toSave.TileLookup.Values);
        PlayerTile = toSave.PlayerPositionTile;
    }
    public void GenerateTiles(WorldTile parentTile)
    {
        foreach(var tile in TileLayoutSettings)
        {
            Tile t = new Tile();
            t = tile;
            t.InitializeTile(parentTile.TileSprite, parentTile,(int)t.GridPosition.x, (int)t.GridPosition.y);
            parentTile.Tiles.Add(t);
            parentTile.GridPositions[(int)t.GridPosition.x, (int)t.GridPosition.y] = t;
            parentTile.TileLookup.Add(t.GetGameObject(), t);
        }
        parentTile.PlayerPositionTile = PlayerTile;
    }
}
