using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PurchasingBehavior : MonoBehaviour
{
    private static PurchasingBehavior purchase;
    private static Tile PlacementTile;
    private static PlaceableObject objectToPlace;
    public static ConfirmationMenu confirmationMenu;
    public static event Action<PlaceableObject, bool> AdjustItemInventory;

    [Header("Constructor Objects")]
    public GameObject confirmationMenuGameObject;
    public GameObject purchaseMenuGameObject;
    public Image MenuBackground;

    private void Awake()
    {
        if (purchase == null) { purchase = this; } else { Destroy(this); }
        confirmationMenu = new ConfirmationMenu(confirmationMenuGameObject);
        confirmationMenu.ConfirmationButton.onClick.AddListener(()=> ConfirmObjectPlacement());
        confirmationMenu.DenyButton.onClick.AddListener(() => DenyPlacement());
        confirmationMenu.ToggleActivation();
        WorldTile.TileSelected += OnTileSelected;
        MinigameManager.EnteredMode += OnEnteredMode;
    }

    private void Start()
    {
        purchaseMenuGameObject.SetActive(false);
    }

    void OnTileSelected(Tile t)
    {
        PlacementTile = t;
        confirmationMenu.ToggleActivation();
    }

    void OnClickSelectObjectToPlace(PlaceableObject p)
    {
        if (IsValidPurchase(p))
        {
            objectToPlace = p;
        }
    }

    void OnEnteredMode(ActiveGameFunction a)
    {
        if (a == ActiveGameFunction.SHOP)
        {
            TogglePurchaseMenu(true);
        } else if (a == ActiveGameFunction.NONE)
        {
            TogglePurchaseMenu(false);
        }
    }

    public static event Action<Tile, PlaceableObject> ObjectPlacementConfirmed;
    void ConfirmObjectPlacement()
    {
        ObjectPlacementConfirmed(PlacementTile, objectToPlace);
        AdjustItemInventory(objectToPlace, false);
        if (objectToPlace != null)
        {
            ObjectPlacementConfirmed(PlacementTile, objectToPlace);
            AdjustItemInventory(objectToPlace, false);
        }
    }

    void DenyPlacement()
    {
        PlacementTile = null;
        confirmationMenu.ToggleActivation();
    }

    public static void TogglePurchaseMenu() { TogglePurchaseMenu(!purchase.purchaseMenuGameObject.activeSelf); }

    public static void TogglePurchaseMenu(bool active)
    {
        purchase.purchaseMenuGameObject.SetActive(active);
    }

    public static bool IsValidPurchase(PlaceableObject p)
    {
        if (MinigameManager.activeCurrency < p.PurchasePrice) { return false; }
        //check adjacent tiles
        return true;
    }

    private void OnDestroy()
    {
        WorldTile.TileSelected -= OnTileSelected;
        MinigameManager.EnteredMode -= OnEnteredMode;
    }
}

[Serializable]
public class InventorySettings
{
    public List<string> placeableObjectLookups;
    private List<PlaceableObject> itemIndices;
    public List<InventoryListing> inventoryListings;

    public InventorySettings() {
        placeableObjectLookups = new List<string>();
        itemIndices = new List<PlaceableObject>();
        inventoryListings = new List<InventoryListing>();
    }

    public void AdjustInventory(PlaceableObject obj, bool add = true)
    {
        if (Owned(obj))
        {
            int index = itemIndices.IndexOf(obj);
            inventoryListings[index].IncrementAmount(add);
            if (inventoryListings[index].NoneOwned) { inventoryListings.RemoveAt(index); itemIndices.RemoveAt(index); }
        }
        else
        {
            if (add)
            {
                itemIndices.Add(obj);
                inventoryListings.Add(new InventoryListing(obj));
            }
        }
    }

    bool Owned(PlaceableObject obj)
    {
        if (itemIndices.Count <= 0 || inventoryListings.Count <=0) { return false; }
        return itemIndices.Contains(obj);
    }

    [Serializable]
    public struct InventoryListing
    {
        public PlaceableObject item;
        public int amountOwned;

        public InventoryListing(PlaceableObject p)
        {
            item = p;
            amountOwned = 1;
        }

        public void IncrementAmount(bool increase)
        {
            if (increase) { amountOwned++; } else { amountOwned--; }
        }

        public bool NoneOwned { get => amountOwned <= 0; }
    }
   
}

[Serializable]
public struct ConfirmationMenu
{
    public GameObject MenuContainer;
    public Button ConfirmationButton, DenyButton;
    public ConfirmationMenu(GameObject container)
    {
        MenuContainer = container;
        ConfirmationButton = container.transform.Find("ConfirmButton").GetComponent<Button>();
        DenyButton = container.transform.Find("DenyButton").GetComponent<Button>();
    }

    public void ToggleActivation()
    {
        bool currentState = MenuContainer.activeSelf;
        MenuContainer.SetActive(!currentState);
    }
}
