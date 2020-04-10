using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PurchasingBehavior : MonoBehaviour
{
    private static PurchasingBehavior purchase;
    private static Tile PlacementTile;
    public static PlaceableObject objectToPlace;
    public static ConfirmationMenu confirmationMenu;
    public static event Action<PlaceableObject, bool> AdjustItemInventory;

    public static PlaceableObject purchaseToConfirm { get; set; }
    PlaceableObject[] placeablesFromResources;
    public PurchaseButton[] purchaseButtons;

    public PlaceableObject testPlacement;

    [Header("Constructor Objects")]
    public GameObject confirmationMenuGameObject;
    public GameObject purchaseMenuGameObject;
    public GameObject purchaseButtonPrefab;
    public Image MenuBackground;

    private void Awake()
    {
        if (purchase == null) { purchase = this; } else { Destroy(this); }
        confirmationMenu = new ConfirmationMenu(confirmationMenuGameObject);
        confirmationMenu.ConfirmationButton.onClick.AddListener(()=> Purchase());
        confirmationMenu.DenyButton.onClick.AddListener(() => DenyPlacement());
        confirmationMenu.ToggleActivation();
        WorldTile.TileSelected += OnTileSelected;
        MinigameManager.EnteredMode += OnEnteredMode;
        CameraMovement.ZoomedOnObject += OnZoomed;
    }

    private void Start()
    {
        purchaseMenuGameObject.SetActive(false);
        objectToPlace = testPlacement;
        placeablesFromResources = Resources.LoadAll<PlaceableObject>("PlaceableObjects");
        purchaseButtons = new PurchaseButton[placeablesFromResources.Length];
        int temp;
        for (int i = 0; i< placeablesFromResources.Length; i++)
        {
            temp = i;
            GameObject newPurchaseButton = Instantiate(purchaseButtonPrefab, MenuBackground.GetComponent<RectTransform>());
            purchaseButtons[temp] = new PurchaseButton(newPurchaseButton, placeablesFromResources[temp]);
        }
    }

    private void Update()
    {
        if (PlacementTile != null && Input.GetKeyDown(KeyCode.K))
        {
            ConfirmObjectPlacement();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            Purchase();
        }
    }

    void OnTileSelected(Tile t)
    {
        PlacementTile = t;
    }

    void OnZoomed(IZoomOn zoom)
    {

    }

    void Purchase()
    {
        if (IsValidPurchase(purchaseToConfirm))
        {
            MinigameManager.activeCurrency -= purchaseToConfirm.PurchasePrice;
            AdjustItemInventory(purchaseToConfirm, true);
            confirmationMenu.ToggleActivation();
            purchaseToConfirm = null;
        }
    }

    void OnEnteredMode(ActiveGameFunction a)
    {
        if (a == ActiveGameFunction.SHOP)
        {
            TogglePurchaseMenu(true);
        } else
        {
            TogglePurchaseMenu(false);
        }
    }

    public static event Action<Tile, PlaceableObject> ObjectPlacementConfirmed;
    public static void ConfirmObjectPlacement()
    {
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
        CameraMovement.ZoomedOnObject -= OnZoomed;
    }
}

[Serializable]
public class InventorySettings
{
    private List<PlaceableObject> itemIndices;
    public List<InventoryListing> inventoryListings;
    public InventoryListUIObject activeInventoryList;

    public InventorySettings() {
        itemIndices = new List<PlaceableObject>();
        inventoryListings = new List<InventoryListing>();
    }

    public void InitializeInventoryUIObject(GameObject parent, GameObject prefab)
    {
        activeInventoryList = new InventoryListUIObject(parent, prefab);
        activeInventoryList.SetFromInventoryListing(this, true);
        for(int i =0; i< activeInventoryList.inventoryButtons.Count; i++)
        {
            int t = i;
            activeInventoryList.inventoryButtons[t].purchaseButton.onClick.AddListener(()=> OnClickGetObjectToSpawn(t));
        }
    }

    public void AdjustInventory(PlaceableObject obj, bool add = true)
    {
        if (Owned(obj))
        {
            int index = itemIndices.IndexOf(obj);
            inventoryListings[index].AdjustInventoryListing(add);
            activeInventoryList.Adjust(index, "x" + inventoryListings[index].amountOwned.ToString());
            if (inventoryListings[index].NoneOwned) { inventoryListings.RemoveAt(index); itemIndices.RemoveAt(index); activeInventoryList.Adjust(inventoryListings[index].item, false); }
        }
        else
        {
            if (add)
            {
                itemIndices.Add(obj);
                inventoryListings.Add(new InventoryListing(obj));
                activeInventoryList.Adjust(obj, true);
                activeInventoryList.GetAtIndex(itemIndices.Count - 1).purchaseButton.onClick.AddListener(()=>OnClickGetObjectToSpawn(itemIndices.Count - 1));
            }
        }
    }

    public void OnClickGetObjectToSpawn(int index)
    {
        PurchasingBehavior.objectToPlace = itemIndices[index];
        PurchasingBehavior.ConfirmObjectPlacement();
    }

    bool Owned(PlaceableObject obj)
    {
        if (itemIndices.Count <= 0) { return false; }
        return itemIndices.Contains(obj);
    }

    [Serializable]
    public class InventoryListing
    {
        public PlaceableObject item;
        public int amountOwned;

        public InventoryListing(PlaceableObject p)
        {
            item = p;
            amountOwned = 1;
        }

        public InventoryListing(PlaceableObject p, int add)
        {
            item = p;
            amountOwned = 1 + add;
        }

        public void AdjustInventoryListing(bool add)
        {
            if (add) { amountOwned++; } else { amountOwned--; }
        }

        public bool NoneOwned { get => amountOwned <= 0; }
    }
   
}

[Serializable]
public struct InventoryListUIObject
{
    public GameObject parentObject { get; private set; }
    public GameObject prefab;
    public List<PurchaseButton> inventoryButtons;
    public InventoryListUIObject(GameObject parent, GameObject buttonPrefab)
    {
        parentObject = parent;
        prefab = buttonPrefab;
        inventoryButtons = new List<PurchaseButton>();
    }

    public void SetFromInventoryListing(InventorySettings listings, bool initialize)
    {
        if (initialize) { SetFromInventoryListing(listings); } 
    }

    void SetFromInventoryListing(InventorySettings listings)
    {
        for (int i = 0; i < listings.inventoryListings.Count; i++)
        {
            Adjust(listings.inventoryListings[i].item, true);

        }
    }

    public PurchaseButton GetAtIndex(int i)
    {
        return inventoryButtons[i];
    }

    public void Toggle(bool tog = true)
    {
        parentObject.SetActive(tog);
    }

    public void Adjust(int index, string message)
    {
        try
        {
            GetAtIndex(index).additionalTextDisplay.text = message;
        } catch(IndexOutOfRangeException) { Debug.Log("Tried to adjust an inventory listing that was out of bounds."); }
    }

    public void Adjust(PlaceableObject p, bool add)
    {
        if (add)
        {
            GameObject g = GameObject.Instantiate(prefab, parentObject.GetComponent<RectTransform>());
            PurchaseButton pb = new PurchaseButton(g, p, "x 1");
            inventoryButtons.Add(pb);
        } else
        {
            foreach(var b in inventoryButtons)
            {
                if(b.objectForSale == p)
                {
                    PurchaseButton dstry = b;
                    inventoryButtons.Remove(b);
                    GameObject.Destroy(dstry.gameObject);
                    return;
                }
            }
        }
    }
}

[Serializable]
public struct ConfirmationMenu
{
    public GameObject MenuContainer;
    public Button ConfirmationButton, DenyButton;
    public TextMeshProUGUI ConfirmationMessageText;
    string defaultMessage;
    public string Message;
    public ConfirmationMenu(GameObject container)
    {
        MenuContainer = container;
        ConfirmationButton = container.transform.Find("ConfirmButton").GetComponent<Button>();
        DenyButton = container.transform.Find("DenyButton").GetComponent<Button>();
        ConfirmationMessageText = container.GetComponentInChildren<TextMeshProUGUI>();
        defaultMessage = "YOU SURE 'BOUT THIS?";
        Message = defaultMessage;
    }

    public void ToggleActivation()
    {
        bool currentState = MenuContainer.activeSelf;
        MenuContainer.SetActive(!currentState);
        ConfirmationMessageText.text = Message + '\n' + defaultMessage;
    }

    public void SetMessage(string message)
    {
        Message = message;
    }

    public void SetMessage()
    {
        Message = defaultMessage;
    }
}

[Serializable]
public class PurchaseButton
{
    public GameObject gameObject { get; private set; }
    public Button purchaseButton;
    public TextMeshProUGUI additionalTextDisplay;
    public PlaceableObject objectForSale;
    public PurchaseButton(GameObject prefab, PlaceableObject p)
    {
        gameObject = prefab;
        purchaseButton = prefab.GetComponentInChildren<Button>();
        additionalTextDisplay = prefab.GetComponentInChildren<TextMeshProUGUI>();
        objectForSale = p;
        SetComponentValues();
        purchaseButton.onClick.AddListener(()=> OnClickSetObjectToPurchase());
    }

    public PurchaseButton(GameObject prefab, PlaceableObject p, string message)
    {
        gameObject = prefab;
        purchaseButton = prefab.GetComponentInChildren<Button>();
        additionalTextDisplay = prefab.GetComponentInChildren<TextMeshProUGUI>();
        objectForSale = p;
        SetComponentValues(message);
    }

    public void OnClickSetObjectToPurchase()
    {
        PurchasingBehavior.purchaseToConfirm = objectForSale;
        PurchasingBehavior.confirmationMenu.SetMessage("PURCHASING " + objectForSale.ObjectLookup.ToUpper());
        PurchasingBehavior.confirmationMenu.ToggleActivation();
    }

    public void SetComponentValues()
    {
        purchaseButton.GetComponentInChildren<Text>().text = objectForSale.ObjectLookup;
        additionalTextDisplay.text = "$" + objectForSale.PurchasePrice.ToString();
    }

    public void SetComponentValues(string additionalMessage)
    {
        purchaseButton.GetComponentInChildren<Text>().text = objectForSale.ObjectLookup;
        additionalTextDisplay.text = additionalMessage;
    }
}

