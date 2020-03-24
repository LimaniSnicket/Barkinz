using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PurchasingBehavior : MonoBehaviour
{
    private static PurchasingBehavior purchase;
    private static Tile PlacementTile;
    public static List<PlaceableObject> purchasedPlaceableObjects { get; private set; }
    private static PlaceableObject objectToPlace;
    public static ConfirmationMenu confirmationMenu;

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
        Debug.Log("Clicked object place button");
        ObjectPlacementConfirmed(PlacementTile, objectToPlace);
        if (objectToPlace != null)
        {
            ObjectPlacementConfirmed(PlacementTile, objectToPlace);
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
public class PurchaseSettings
{
    public List<string> placeableObjectLookups;
    public PurchaseSettings() {
        placeableObjectLookups = new List<string>();

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
