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
    public UtilityCharacterInformation MeerkatMac;
    Dictionary<string, ActiveGameFunction> GameFunctionToTagLookup;
    ActiveGameFunction toEnter;

    public static bool SwappingMode { get; private set; }

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

    public static event Action<ActiveGameFunction> EnteredMode;

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

        if (Input.GetKeyDown(KeyCode.P) && activeGameFunction == ActiveGameFunction.NONE)
        {
            activeGameFunction = ActiveGameFunction.SHOP;
            EnteredMode(ActiveGameFunction.SHOP);
        }

        if (CanEnterMode())
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                StartCoroutine(EnterMode(toEnter));
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                StartCoroutine(EnterMode(ActiveGameFunction.DARTS));
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitMode();
        }
    }

    public static void ExitMode()
    {
        gameManager.activeGameFunction = ActiveGameFunction.NONE;
        EnteredMode(ActiveGameFunction.NONE);
        CameraMovement.AlignWithTransform();
    }

    public static void RewardPlayer(float amount)
    {
        activeCurrency += Mathf.Round(amount);
    }

    void OnDrinkTab(float t)
    {
        activeCurrency -= t;
    }

    public static bool CanEnterMode()
    {
        return !SwappingMode && ValidMode(ActiveGameFunction.NONE);
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

    public IEnumerator EnterMode(ActiveGameFunction toEnter)
    {
        SwappingMode = true;
        if (toEnter == ActiveGameFunction.BAR) { yield return StartCoroutine(MacPopUp(true, 3f)); }
        Debug.Log("Swapping Modes");
        yield return new WaitForSeconds(1);
        activeGameFunction = toEnter;
        EnteredMode(activeGameFunction);
        Debug.Log("Mode Swapped");
        SwappingMode = false;
    }

    IEnumerator MacPopUp(bool poppingUp, float speed)
    {
        Vector3 goTo = poppingUp ? MeerkatMac.EndPos : MeerkatMac.StartPos;
        while (!MeerkatMac.OverworldCharacter.transform.position.SqueezeVectors(goTo))
        {
            MeerkatMac.OverworldCharacter.transform.position = Vector3.Lerp(MeerkatMac.OverworldCharacter.transform.position, goTo, Time.deltaTime * speed);
            yield return null;
        }
        MeerkatMac.OverworldCharacter.transform.position = goTo;
    }

    private void OnDestroy()
    {
        BarkinzManager.InitializeBarkinzData -= SetCurrencyOnInitialize;
        Bartender.DrinkOnTab -= OnDrinkTab;
        ActivePlayer.EnteredTaggedArea -= OnEnteredTaggedArea;
    }
}

[System.Serializable]
public struct UtilityCharacterInformation
{
    public GameObject OverworldCharacter;
    public Vector3 StartPos, EndPos;
}

public enum ActiveGameFunction
{
    NONE = 0,
    SHOP = 1,
    BAR = 2,
    DARTS = 3,
    TRIVIA = 4
}
