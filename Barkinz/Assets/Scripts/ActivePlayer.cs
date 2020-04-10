using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ActivePlayer : MonoBehaviour, IZoomOn
{
    public Tile TargetTile;
    Queue<Tile> tileTargets;

    public SpriteRenderer OverworldSpriteDisplay;

    public IntoxicationSettings ActiveSessionIntoxication;
    public InventorySettings activeInventory;
    public static event Action<string> EnteredTaggedArea;
    public static event Action<ActivePlayer> SetActivePlayer;
    public float CameraOrthoSize { get; set; }
    public Transform ZoomObjectTransform { get => transform; }

    public GameObject purchaseButtonPrefab;
    public GameObject inventoryUIObject;

    private void Awake()
    {
        tileTargets = new Queue<Tile>();
        WorldTile.TileSelected += OnTileSelected;
        WorldTile.QueueTile += OnTileQueue;
        BarkinzManager.InitializeBarkinzData += OnBarkinzInitialization;
        PurchasingBehavior.AdjustItemInventory += OnInventoryAdjustment;
        MinigameManager.EnteredMode += OnEnteredMode;
    }

    private void Start()
    {
        SetActivePlayer(this);
        OverworldSpriteDisplay = GetComponentInChildren<SpriteRenderer>();
        CameraOrthoSize = 1;
        activeInventory.activeInventoryList.Toggle(false);
    }

    public float chugSpeed = 0;
    private void Update()
    {
        if (Input.GetKey(KeyCode.Space) && MinigameManager.ValidMode(ActiveGameFunction.NONE))
        {
            ChugDrink();
        } else
        {
            ActiveSessionIntoxication.SoberUp();
            chugSpeed = 0;
        }
    }

    void ChugDrink()
    {
        if (Bartender.DrinkActive())
        {
            chugSpeed += Time.deltaTime * .75f;
            Bartender.currentDrink.SipDrink(Mathf.Max(chugSpeed, 2));
            ActiveSessionIntoxication.Intoxicate(chugSpeed, Bartender.currentDrink);
        }
    }

    void OnInventoryAdjustment(PlaceableObject obj, bool add)
    {
        if (add) { activeInventory.AdjustInventory(obj); } else
        {
            activeInventory.AdjustInventory(obj, false);
            //Debug.Log("And <i>this</i> is where I'd adjust player's inventory!" + '\n' + "<b>IF I HAD IMPLEMENTED IT</b>");
        }
    }

    void OnEnteredMode(ActiveGameFunction gameFunction)
    {
        if (gameFunction == ActiveGameFunction.FOCUS) {
            activeInventory.activeInventoryList.Toggle();
        } else { activeInventory.activeInventoryList.Toggle(false); }
    }

    void OnBarkinzInitialization(BarkinzInfo b)
    {
        ActiveSessionIntoxication = new IntoxicationSettings();
        activeInventory = new InventorySettings();
        if (b.LoadSettingsFromInfo)
        {
            b.SetPlayerPosition(this);
            b.SetIntoxicationData(this);
        }
        OverworldSpriteDisplay.sprite = b.OverworldSprite;
        activeInventory.InitializeInventoryUIObject(inventoryUIObject, purchaseButtonPrefab);
    }

    void SetBarkinzIntoxicationData(BarkinzInfo b)
    {
        b.UpdateIntoxicationSettings(this);
    }

    void SetBarkinzInventoryData(BarkinzInfo b)
    {
        b.UpdateInventorySettings(activeInventory);
    }

    void OnTileSelected(Tile t)
    {

    }

    void OnTileQueue(Tile t)
    {
        tileTargets.Enqueue(t);
        if (tileTargets.Count == 1) { StartCoroutine(MoveThroughTargetQueue()); }
    }

    public IEnumerator MoveThroughTargetQueue()
    {
        while (tileTargets.Count > 0)
        {
            yield return StartCoroutine(MoveToTargetTile());
        }
    }

    IEnumerator MoveToTargetTile()
    {
        TargetTile.occupied = false;
        TargetTile = tileTargets.Peek();
        while (Vector3.Distance(transform.position, TargetTile.centerPosition) >= 0.001f)
        {
            transform.position = Vector3.Lerp(transform.position, TargetTile.centerPosition, Time.deltaTime * 20);
            yield return null;
        }
        transform.position = TargetTile.centerPosition;
        TargetTile.occupied = true;
        tileTargets.Dequeue();
    }

    private void OnTriggerEnter(Collider collision)
    {
        EnteredTaggedArea(collision.tag);
    }

    private void OnDestroy()
    {
        WorldTile.TileSelected -= OnTileSelected;
        BarkinzManager.InitializeBarkinzData -= OnBarkinzInitialization;
        PurchasingBehavior.AdjustItemInventory -= OnInventoryAdjustment;
        MinigameManager.EnteredMode -= OnEnteredMode;
    }

    private void OnApplicationQuit()
    {
        SetBarkinzIntoxicationData(BarkinzManager.PrimaryBarkinz);
        SetBarkinzInventoryData(BarkinzManager.PrimaryBarkinz);
    }

    public Vector3 ZoomCamPosition()
    {
        Vector3 p = transform.position + new Vector3(0, 1, -1);
        return p;
    }
}
