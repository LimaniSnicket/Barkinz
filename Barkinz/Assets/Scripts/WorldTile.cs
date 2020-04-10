using UnityEngine;
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

    public Tile PlayerPositionTile, StartTile;
    private Tile mouseHoverTile;

    public GameObject PlaceableObjectPrefab;
    public PlaceableObject TestPlacements;
    public List<PlacedObject> ObjectsInTile;

    private void Awake()
    {
        BarkinzManager.InitializeBarkinzData += OnBarkinzLoad;
        PurchasingBehavior.ObjectPlacementConfirmed += OnObjectPlacementConfirmed;
        PlayerPositionTile = new Tile();
        mouseHoverTile = new Tile();
    }

    private void Update()
    {
        StartTile.isStartingTile = true;
        if (Input.GetMouseButtonDown(1))
        {
            CameraMovement.ResetCameraZoom();
            if (MinigameManager.ValidMode(ActiveGameFunction.FOCUS)) { MinigameManager.ExitMode(); }
        }

        if (Input.GetMouseButtonDown(0) && MinigameManager.ValidMode(ActiveGameFunction.NONE))
        {
            if (Tiles.Contains(mouseHoverTile))
            {
                MinigameManager.EnterGameMode(ActiveGameFunction.FOCUS);
                SetTargetTileViaClick(mouseHoverTile);
                CameraMovement.ZoomOn(mouseHoverTile);
            }
        }

        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition - (Vector3.up * -.45f));
        RaycastHit rh = new RaycastHit();
        if (Physics.SphereCast(r, .5f, out rh, Mathf.Infinity) && TileLookup.ContainsKey(rh.transform.gameObject))
        {
            Tile t = TileLookup[rh.transform.gameObject];
            if (mouseHoverTile != t)
            {
                mouseHoverTile.SetSpriteColor(new Color(1, 1, 1, 0.3f));
                mouseHoverTile = t;
                mouseHoverTile.SetSpriteColor(new Color(0, 0, 1, 0.5f));
            }
        } else
        {
            mouseHoverTile.SetSpriteColor(new Color(1, 1, 1, 0.3f));
            mouseHoverTile = new Tile();
        }

        if (MinigameManager.ValidMode(ActiveGameFunction.NONE) && MinigameManager.AcceptPlayerInput)
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                UpdatePlayerTile(GetAdjacentTile(Vector2.right));
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                UpdatePlayerTile(GetAdjacentTile(Vector2.right * -1));
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                UpdatePlayerTile(GetAdjacentTile(Vector2.up * -1));
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                UpdatePlayerTile(GetAdjacentTile(Vector2.up));
            }
        }
    }

    void OnObjectPlacementConfirmed(Tile t, PlaceableObject p)
    {
        Debug.Log("Instantiate Object");
        InstantiatePlacedObject(TestPlacements, t.GridPosition);
    }

    public void InstantiatePlacedObject(PlaceableObject po, Vector2Int grid)
    {
        PlacedObject p = Instantiate(PlaceableObjectPrefab).GetComponent<PlacedObject>();
        p.InitializePlacedObject(grid, po);
        Tile placedAt = GetTileAtPosition((int)p.GridPosition.x, (int)p.GridPosition.y);
        placedAt.SetPlacedObjectPosition(p);
        ObjectsInTile.Add(p);
    }

    public void InstantiatePlacedObjects(WorldTileSettings wts)
    {
        if (wts.ObjectPlacementData != null && wts.ObjectPlacementData.Count > 0) {
            foreach (var e in wts.ObjectPlacementData)
            {
                PlaceableObject p = Resources.Load<PlaceableObject>("PlaceableObjects/" + e.resourcesPath);
                InstantiatePlacedObject(p, e.gridPosition);
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

    void UpdatePlayerTile(Tile newTile)
    {
        PlayerPositionTile.occupied = false;
        PlayerPositionTile = newTile;
        PlayerPositionTile.occupied = true;
    }

    public static event Action<Tile> TileSelected;
    public static event Action<Tile> QueueTile;


    public void SetTargetTileViaClick(Tile t)
    {
        Debug.Log("Clicked on Tile");
        TileSelected(t);
    }

    void OnBarkinzLoad(BarkinzInfo b)
    {
        TileLookup = new Dictionary<GameObject, Tile>();
        Tiles = new List<Tile>();
        ObjectsInTile = new List<PlacedObject>();
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
        PlayerPositionTile = GridPositions[Median(0, tileColumns), 0];
        StartTile = PlayerPositionTile;
    }

    void UpdatePrimaryBarkinzOnQuit(BarkinzInfo b)
    {
        b.UpdateWorldTileSettings(this);
    }

    int Median(int low, int high) { return (Mathf.CeilToInt((low + high) / 2)); }

    private void OnDestroy()
    {
        BarkinzManager.InitializeBarkinzData -= OnBarkinzLoad;
        PurchasingBehavior.ObjectPlacementConfirmed -= OnObjectPlacementConfirmed;
    }

    private void OnApplicationQuit()
    {
        UpdatePrimaryBarkinzOnQuit(BarkinzManager.PrimaryBarkinz);
    }
}

[Serializable]
public class Tile : IZoomOn
{
    private GameObject go;
    private SpriteRenderer TileRenderer;
    public float width, length;
    public bool occupied, isStartingTile;
    public Vector3 centerPosition;
    public Vector2Int GridPosition { get; private set; }

    public GameObject occupyingTile;

    public Transform ZoomObjectTransform => go.transform;
    public float CameraOrthoSize => 3;
    public bool placeableTile { get => !occupied && !isStartingTile; }

    public static event Action<PlaceableObject> RemovedPlacedObjectFromTile;

    public Tile() { width = 1; length = 1; occupied = false; centerPosition = Vector3.zero; }
    public Tile(Vector3 cp) { width = 1; length = 1; occupied = false; centerPosition = cp; }
    public Tile(float w, float l, Vector3 cp) { width = w; length = l; centerPosition = cp; occupied = false; GridPosition = new Vector2Int(0, 0); }
    public Tile(float w, float l, Vector3 cp, int row, int column) { width = w; length = l; centerPosition = cp; occupied = false; GridPosition = new Vector2Int(row, column); }


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
        TileRenderer.color = new Color(1, 1, 1, 0.3f);
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
        TileRenderer.color = new Color(1,1,1,0.3f);
        g.transform.SetParent(parentTile.transform);
        GridPosition = new Vector2Int(i, j);
    }

    public GameObject GetGameObject()
    {
        return go;
    }

    public void SetPlacedObjectPosition(PlacedObject p)
    {
        p.transform.position = centerPosition;
        occupied = true;
        occupyingTile = p.gameObject;
    }

    public void RemovePlacedObject(PlacedObject p)
    {
        RemovedPlacedObjectFromTile(p.ObjectInformation);
        occupied = false;
        occupyingTile = null;
    }

    public void SetSpriteColor(Color c)
    {
        if(TileRenderer == null) { return; }
        TileRenderer.color = c;
    }

    public Vector3 ZoomCamPosition()
    {
        Vector3 p = go.transform.position;
        return p + new Vector3(-1, .5f, -1);
    }
}

[Serializable]
public class WorldTileSettings
{
    public List<Tile> TileLayoutSettings;
    public Tile PlayerTile, StartTile;
    public Vector2Int playerPosition;
    public List<ObjectPlacementInfo> ObjectPlacementData;
    public WorldTileSettings() { TileLayoutSettings = new List<Tile>(); }
    public WorldTileSettings(WorldTile toSave)
    {
        TileLayoutSettings = new List<Tile>(toSave.TileLookup.Values);
        PlayerTile = toSave.PlayerPositionTile;
        StartTile = toSave.StartTile;
        playerPosition = toSave.StartTile.GridPosition;//toSave.PlayerPositionTile.GridPosition;
        ObjectPlacementData = new List<ObjectPlacementInfo>();
        foreach (var o in toSave.ObjectsInTile)
        {
            ObjectPlacementData.Add(new ObjectPlacementInfo(o));
        }
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
        parentTile.PlayerPositionTile = parentTile.GetTileAtPosition(playerPosition.x, playerPosition.y);
    }

    public void InitializeWorld(WorldTile t)
    {
        t.InstantiatePlacedObjects(this);
    }

    [Serializable]
    public struct ObjectPlacementInfo
    {
        public string resourcesPath;
        public Vector2Int gridPosition;
        public ObjectPlacementInfo(PlacedObject p)
        {
            resourcesPath = p.ObjectInformation.ObjectLookup;
            gridPosition = p.GridPosition;
        }
    }
}
