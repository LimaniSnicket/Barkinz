using System;
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


    void Awake()
    {
        MinigameManager.EnteredMode += OnModeChange;
    }

    private void Start()
    {
        if(bartender == null) { bartender = this; } else { Destroy(this); }
        currentDrink = new Drink();
        orderedDrinkBacklog = new List<Drink>();
        OrderButtons = new List<OrderDrinkButton>();
        Debug.Log(DrinkMenu.NumberOfDrinks);
        for (int i = 0; i < DrinkMenu.NumberOfDrinks; i++)
        {
            Button b = Instantiate(ButtonPrefab, ButtonPanel.transform).GetComponent<Button>();
            OrderDrinkButton odb = new OrderDrinkButton(b, DrinkMenu.GetDrinks()[i]);
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
    public Drink() { drinkName = "Water"; drinkCost = 0; drinkStrength = -1; available = true; amountLeft = 5; displaySpritePath = "DrinkSprites/Temp"; }
    public Drink(string name, float cost, float strength, float amount, bool a = true)
    {
        drinkName = name;
        drinkCost = cost;
        drinkStrength = strength;
        amountLeft = amount;
        available = a;
        displaySpritePath = "DrinkSprites/Temp";
    }

    public Drink(string name, float cost, float strength, float amount, string spritePath, bool a = true)
    {
        drinkName = name;
        drinkCost = cost;
        drinkStrength = strength;
        amountLeft = amount;
        available = a;
        displaySpritePath = spritePath;
    }

    public Drink(Drink copy)
    {
        drinkName = copy.drinkName;
        drinkCost = copy.drinkCost;
        drinkStrength = copy.drinkStrength;
        amountLeft = copy.amountLeft;
        available = copy.available;
        displaySpritePath = copy.displaySpritePath;
    }

    public string displaySpritePath;
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
        b.GetComponentInChildren<Text>().text = orderDrink.drinkName;
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

public static class DrinkMenu
{
    public static List<Drink> GetDrinks()
    {
        List<Drink> drinks = new List<Drink>();
        drinks.Add(Water); drinks.Add(FourLoko);
        drinks.Add(Hennessy); drinks.Add(Chianti);
        drinks.Add(Fireball); drinks.Add(Ouzo);
        drinks.Add(Whiteclaw);
        return drinks;
    }

    public static int NumberOfDrinks { get => GetDrinks().Count; }

    public static Drink Water { get => new Drink(); }
    public static Drink FourLoko { get => new Drink("Four Loko", 4, 8, 8); }
    public static Drink Hennessy { get => new Drink("Hennessy", 10, 7, 4, "DrinkSprites/Henny"); }
    public static Drink Chianti { get => new Drink("Chianti", 10, 1, 4); }
    public static Drink Fireball { get => new Drink("Fireball", 5, 6, 4); }
    public static Drink Ouzo { get => new Drink("Ouzo", 10, 10, 4); }
    public static Drink Whiteclaw { get => new Drink("Whiteclaw", 5, 4, 7); }
}
