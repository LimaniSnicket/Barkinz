using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
