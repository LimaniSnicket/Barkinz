using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bartender : MonoBehaviour, IGameMode
{
    private static Bartender bartender;
    public static Drink currentDrink;
    public static Stack<Drink> orderedDrinkBacklog { get; private set; }
    static Stack<DrinkBacklogObject> backlog;
    public Drink debugCurrent;
    public GameObject ButtonPrefab, ButtonPanel, DrinkDisplayObject, backlogContainer, backlogObject;

    public List<OrderDrinkButton> OrderButtons;
    public DrinkDisplay DrinkDisplay;
   
    public ActiveGameFunction GameModeFunction { get => ActiveGameFunction.BAR; }
    ActivePlayer player;

    public static event Action<float> DrinkOnTab;
    public DrinkMenu drinkMenu;


    void Awake()
    {
        string drinkPath = Application.streamingAssetsPath + "/DrinkData.json";
        InitializeDrinkMenu(drinkPath);
        MinigameManager.EnteredMode += OnModeChange;
        BarkinzManager.InitializeBarkinzData += OnBarkinzLoad;
        ActivePlayer.SetActivePlayer += OnSetActivePlayer;
    }

    private void Start()
    {
        if(bartender == null) { bartender = this; } else { Destroy(this); }
        orderedDrinkBacklog = new Stack<Drink>();
        backlog = new Stack<DrinkBacklogObject>();
        OrderButtons = new List<OrderDrinkButton>();
        for (int i = 0; i < drinkMenu.drinkData.Count; i++)
        {
            Button b = Instantiate(ButtonPrefab, ButtonPanel.transform).GetComponent<Button>();
            OrderDrinkButton odb = new OrderDrinkButton(b, drinkMenu.drinkData[i]);
            odb.orderButton.onClick.AddListener(() => OnClickOrderDrink(odb.orderDrink));
            OrderButtons.Add(odb);
        }
        DrinkDisplay = new DrinkDisplay(DrinkDisplayObject, currentDrink);
    }

    private void Update()
    {
        debugCurrent = currentDrink;
        DrinkDisplay.RunDrinkDisplay(currentDrink);
        if (!noDrinkActive)
        {
            if (currentDrink.FinishedDrink)
            {
               if(orderedDrinkBacklog.Count > 0) { currentDrink = RemoveDrinkFromBacklog(); DrinkDisplay.RefreshDrinkDisplay(currentDrink); } else { currentDrink = null; }
            }
        }
    }

    public static bool noDrinkActive { get => currentDrink == null; }

    void OnClickOrderDrink(Drink d)
    {
        if (!noDrinkActive) { AddDrinkToBacklog(currentDrink); }
        currentDrink = new Drink(d);
        if (d.Affordable) { MinigameManager.activeCurrency -= d.drinkCost; } else
        {
            DrinkOnTab(d.DifferenceForTab);
            MinigameManager.activeCurrency = 0;
        }
        DrinkDisplay.RefreshDrinkDisplay(d);
        Debug.Log("Now drinking: " + currentDrink.drinkName);
    }

    public static bool DrinkActive()
    {
        if(currentDrink != null && !currentDrink.FinishedDrink) { return true; }
        return false;
    }

    public void OnModeChange(ActiveGameFunction entered)
    {
        if (entered == GameModeFunction)
        {
            Debug.Log("Bartender Mode");
        }
    }

    void AddDrinkToBacklog(Drink d)
    {
        orderedDrinkBacklog.Push(d);
        GameObject obj = Instantiate(backlogObject, backlogContainer.GetComponent<RectTransform>());
        DrinkBacklogObject b = new DrinkBacklogObject(obj, d);
        backlog.Push(b);
    }

    Drink RemoveDrinkFromBacklog()
    {
        if (orderedDrinkBacklog.Count > 0)
        {
            GameObject g = backlog.Pop().gameObject;
            Destroy(g);
            return orderedDrinkBacklog.Pop();
        } else
        {
            return null;
        }
    }

    void OnSetActivePlayer(ActivePlayer p)
    {
        player = p;
        
    }

    void OnBarkinzLoad(BarkinzInfo primary)
    {
        currentDrink = new Drink();
        Drink pref = drinkMenu.GetDrinkFromCode(primary);
        Debug.Log(pref.drinkName);
        currentDrink = new Drink(pref);
    }

    void InitializeDrinkMenu(string path)
    {
        string json = File.ReadAllText(path);
        drinkMenu = JsonUtility.FromJson<DrinkMenu>(json);
    }


    void OnDestroy()
    {
        MinigameManager.EnteredMode -= OnModeChange;
        ActivePlayer.SetActivePlayer -= OnSetActivePlayer;
    }
}

[Serializable]
public class Drink
{
    public string drinkName;
    public float drinkCost, drinkStrength;
    public float amountLeft;
    public bool available;
    public Drink() { }

    public Drink(Drink copy)
    {
        drinkName = copy.drinkName;
        drinkCost = copy.drinkCost;
        drinkStrength = copy.drinkStrength;
        amountLeft = copy.amountLeft;
        available = copy.available;
        displaySpritePath = copy.displaySpritePath;
        prefBarkinzPath = copy.prefBarkinzPath;
    }

    public string displaySpritePath, prefBarkinzPath;
    public bool FinishedDrink { get => amountLeft <= 0; }
    public void SipDrink(float mod) { amountLeft -= Time.deltaTime * mod; }
    public bool Affordable { get => drinkCost <= MinigameManager.activeCurrency; }
    public float DifferenceForTab { get => drinkCost - MinigameManager.activeCurrency; }
}

[Serializable]
public struct OrderDrinkButton
{
    public Button orderButton;
    public Drink orderDrink;
    public OrderDrinkButton(Button b, Drink d)
    {
        orderButton = b;
        orderDrink = d;
        b.GetComponentInChildren<Text>().text = orderDrink.drinkName + ": $" + orderDrink.drinkCost;
    }
}
[Serializable]
public struct DrinkDisplay
{
    public Drink drinkToDisplay;
    public Image drinkDisplay;
    public Slider fillDisplay;
    public DrinkDisplay(Drink d, Image i, Slider s)
    {
        drinkToDisplay = d;
        drinkDisplay = i;
        fillDisplay = s;
        drinkDisplay.sprite = Resources.Load<Sprite>(d.displaySpritePath);
        fillDisplay.maxValue = drinkToDisplay.amountLeft;
        fillDisplay.minValue = 0;
    }

    public DrinkDisplay(GameObject g, Drink d)
    {
        drinkToDisplay = d;
        drinkDisplay = g.GetComponent<Image>();
        fillDisplay = g.GetComponentInChildren<Slider>();
        drinkDisplay.sprite = Resources.Load<Sprite>(d.displaySpritePath);
        fillDisplay.maxValue = drinkToDisplay.amountLeft;
        fillDisplay.minValue = 0;
    }

    public void RefreshDrinkDisplay(Drink d)
    {
        drinkToDisplay = d;
        drinkDisplay.sprite = Resources.Load<Sprite>(d.displaySpritePath);
        fillDisplay.maxValue = drinkToDisplay.amountLeft;
    }

    public void RunDrinkDisplay()
    {
        fillDisplay.value = drinkToDisplay.amountLeft;
    }

    public void RunDrinkDisplay(Drink d)
    {
        if (!Bartender.noDrinkActive)
        {
            fillDisplay.value = d.amountLeft;
        }
        fillDisplay.gameObject.SetActive(!Bartender.noDrinkActive);
        drinkDisplay.gameObject.SetActive(!Bartender.noDrinkActive);
    }
}

[Serializable]
public struct DrinkBacklogObject
{
    public GameObject gameObject { get; private set; }
    public Image drinkSprite;
    public DrinkBacklogObject(GameObject g, Drink d)
    {
        gameObject = g;
        drinkSprite = g.transform.Find("Drink Sprite").GetComponent<Image>();
        drinkSprite.sprite = Resources.Load<Sprite>(d.displaySpritePath);
    }
}

[Serializable]
public class DrinkMenu
{
    public List<Drink> drinkData;
    public Drink GetDrinkFromCode(BarkinzInfo b)
    {
        foreach (Drink d in drinkData)
        {
            if (d.prefBarkinzPath == b.BarkinzCode) { return d; }
        }
        return drinkData[0];
    }

    public Drink GetDrinkFromName(string name)
    {
        foreach(Drink d in drinkData) { if (d.drinkName == name) { return d; } }
        return drinkData[0];
    }
}
