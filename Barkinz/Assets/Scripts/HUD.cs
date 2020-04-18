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
    public static void PulseText(this RectTransform rt, float minSize, float modifier, float speed)
    {
        Vector3 scale = (Mathf.Sin(Time.time * Mathf.PI * speed) + 1) * modifier * Vector3.one;
        rt.localScale = Vector3.one * minSize + scale;
    }
}
