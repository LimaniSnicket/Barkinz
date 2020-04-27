using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HUD : MonoBehaviour
{
    public Button QuitToTitle;
    public TextMeshProUGUI CurrencyDisplay;
    public Slider IntoxicationSlider;
    public ActivePlayer player;

    private void Start()
    {
        IntoxicationSlider.maxValue = 100;
        if(QuitToTitle!= null) { QuitToTitle.onClick.AddListener(()=>BarkinzManager.OnClickQuitToMainMenu()); }
    }

    private void Update()
    {
        IntoxicationSlider.value = player.ActiveSessionIntoxication.intoxicationLevel;
        CurrencyDisplay.text = "$" + MinigameManager.activeCurrency.ToString();
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
}
