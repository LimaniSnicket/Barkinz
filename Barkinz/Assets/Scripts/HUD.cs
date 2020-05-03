using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HUD : MonoBehaviour
{
    public Button QuitToTitle, SaveGame;
    public TextMeshProUGUI CurrencyDisplay;
    public Slider IntoxicationSlider;
    public ActivePlayer player;
    public ConfirmationMenu ForfeitPopup;
    public GameObject confirmationMenuPrefab;
    public static event Action<ActiveGameFunction> ForfeitGameFunction;
    public delegate void SaveGameFunc();
    public static event SaveGameFunc OnClickSaveData;

    private void Start()
    {
        IntoxicationSlider.maxValue = 100;
        if(QuitToTitle!= null) { QuitToTitle.onClick.AddListener(()=>BarkinzManager.OnClickQuitToMainMenu()); }
        if(SaveGame != null) { SaveGame.onClick.AddListener(() => SaveOnClick()); }
        ForfeitPopup = new ConfirmationMenu(confirmationMenuPrefab, "FORFEITING GAME--");
        ForfeitPopup.ConfirmationButton.onClick.AddListener(()=> OnClickForfeit());
        ForfeitPopup.DenyButton.onClick.AddListener(() => OnClickCloseMenu());
        ForfeitPopup.ToggleActivation();
    }

    private void Update()
    {
        IntoxicationSlider.value = player.ActiveSessionIntoxication.intoxicationLevel;
        CurrencyDisplay.text = "$" + MinigameManager.activeCurrency.ToString();
        if (MinigameManager.waitForForfeitPopupResolution) { ForfeitPopup.MenuContainer.SetActive(true); }
    }

    void SaveOnClick()
    {
        if (OnClickSaveData != null)
        {
            OnClickSaveData();
        }
    }

    void OnClickCloseMenu()
    {
        ForfeitPopup.ToggleActivation(false);
        MinigameManager.waitForForfeitPopupResolution = false;
    }

    void OnClickForfeit()
    {
        ForfeitGameFunction(MinigameManager.getActiveGameFunction);
        MinigameManager.ExitMode();
        MinigameManager.waitForForfeitPopupResolution = false;
        ForfeitPopup.ToggleActivation(false);
    }
}

public static class HelperFunctions
{
    public static void PulseText(this RectTransform rt, float minSize, float modifier, float frequency, float amplitude = 1, float offset = 1)
    {
        Vector3 scale = Vector3.one * (amplitude * Mathf.Sin(Time.time * 2 * Mathf.PI * frequency) + offset);
        rt.localScale = Vector3.one * minSize + (scale * modifier);
    }

    public static float PulseValue(float minValue, float modifier, float frequency, float amplitude = 1, float offset = 1)
    {
        float sin = amplitude * (Mathf.Sin(Time.time * 2 * Mathf.PI * frequency)) + offset;
        return minValue + (sin * modifier);
    }

    public static Color Spectrum()
    {
        float r = PulseValue(.5f, 1, .3f);
        float g = PulseValue(0, 1, .1f);
        float b = PulseValue(.2f, 1, .4f);
        return new Color(r, g, b, 1);
    }

    public static Color Spectrum(float rMin, float rFreq, float gMin, float gFreq, float bMin, float bFreq)
    {
        float r = PulseValue(rMin, 1, rFreq);
        float g = PulseValue(gMin, 1, gFreq);
        float b = PulseValue(bMin, 1, bFreq);
        return new Color(r, g, b, 1);
    }

    public static float PercentTotal(float current, float max)
    {
        if(current <= 0) { return 0; }
        return current * 100 / Mathf.Max(max, 1);
    }
}
