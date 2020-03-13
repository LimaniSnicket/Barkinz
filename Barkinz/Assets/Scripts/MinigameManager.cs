using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MinigameManager : MonoBehaviour
{
    private static MinigameManager gameManager;
    public ActiveGameFunction activeGameFunction;

    public static float activeCurrency;
    private const float startingAmount = 50f;

    public GameObject BarCanvas, TriviaCanvas;
    Dictionary<string, ActiveGameFunction> GameFunctionToTagLookup;
    ActiveGameFunction toEnter;

    private void Awake()
    {
        BarkinzManager.InitializeBarkinzData += SetCurrencyOnInitialize;
        Bartender.DrinkOnTab += OnDrinkTab;
        ActivePlayer.EnteredTaggedArea += OnEnteredTaggedArea;
    }

    private void Start()
    {
        if(gameManager == null) { gameManager = this; } else { Destroy(this); }
        activeGameFunction = 0;
        BarCanvas.gameObject.SetActive(false);
        TriviaCanvas.SetActive(false);
        GameFunctionToTagLookup = new Dictionary<string, ActiveGameFunction>();
        GameFunctionToTagLookup.Add("Bar", ActiveGameFunction.BAR);
        GameFunctionToTagLookup.Add("Trivia",ActiveGameFunction.TRIVIA);
    }

    public void Update()
    {
        BarCanvas.SetActive(ValidMode(ActiveGameFunction.BAR));
        TriviaCanvas.SetActive(ValidMode(ActiveGameFunction.TRIVIA));

        if (Input.GetKeyDown(KeyCode.B) && activeGameFunction == ActiveGameFunction.NONE)
        {
            activeGameFunction = ActiveGameFunction.BAR;
        }
        if (Input.GetKeyDown(KeyCode.T) && activeGameFunction == ActiveGameFunction.NONE)
        {
            activeGameFunction = ActiveGameFunction.TRIVIA;
        }

        if (Input.GetKeyDown(KeyCode.L) && activeGameFunction == ActiveGameFunction.NONE)
        {
            activeGameFunction = toEnter;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            activeGameFunction = ActiveGameFunction.NONE;
        }
    }

    public static void RewardPlayer(float amount)
    {
        activeCurrency += Mathf.Round(amount);
    }

    void OnDrinkTab(float t)
    {
        activeCurrency -= t;
    }

    public static bool ValidMode(ActiveGameFunction gameFunction)
    {
        return gameManager.activeGameFunction == gameFunction;
    }

    void SetCurrencyOnInitialize(BarkinzInfo b)
    {
        if (b.LoadSettingsFromInfo)
        {
            activeCurrency = b.currencyOwned;
        } else
        {
            activeCurrency = startingAmount;
        }
    }

    void OnEnteredTaggedArea(string t)
    {
        if (GameFunctionToTagLookup.ContainsKey(t))
        {
            toEnter = GameFunctionToTagLookup[t];
        }
    }

    private void OnDestroy()
    {
        BarkinzManager.InitializeBarkinzData -= SetCurrencyOnInitialize;
        Bartender.DrinkOnTab -= OnDrinkTab;
        ActivePlayer.EnteredTaggedArea -= OnEnteredTaggedArea;
    }
}

public enum ActiveGameFunction
{
    NONE = 0,
    SHOP = 1,
    BAR = 2,
    DARTS = 3,
    TRIVIA = 4
}
