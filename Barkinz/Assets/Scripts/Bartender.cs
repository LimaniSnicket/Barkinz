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
    public List<Drink> orderedDrinkBacklog;
    public Drink debugCurrent;
    public GameObject ButtonPrefab, ButtonPanel, DrinkDisplayObject;

    public List<OrderDrinkButton> OrderButtons;
    public DrinkDisplay DrinkDisplay;

    public ActiveGameFunction GameModeFunction { get => ActiveGameFunction.BAR; }

    public static event Action<float> DrinkOnTab;
    public DrinkMenu drinkMenu;


    void Awake()
    {
        string drinkPath = Application.streamingAssetsPath + "/DrinkData.json";
        InitializeDrinkMenu(drinkPath);
        MinigameManager.EnteredMode += OnModeChange;
        BarkinzManager.InitializeBarkinzData += OnBarkinzLoad;
    }

    private void Start()
    {
        if(bartender == null) { bartender = this; } else { Destroy(this); }
        orderedDrinkBacklog = new List<Drink>();
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
    }

    void OnClickOrderDrink(Drink d)
    {
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
        fillDisplay.value = d.amountLeft;
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
}
