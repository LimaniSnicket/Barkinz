using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName ="New Barkinz", menuName = "Barkinz")]
public class BarkinzInfo : ScriptableObject
{
    public string BarkinzType;
    public string BarkinzCode;

    public Sprite MainBarkinzSprite;
    public Sprite OverworldSprite;

    public bool LoadSettingsFromInfo;
    public float currencyOwned;
    [SerializeField] private WorldTileSettings individualSettings;
    [SerializeField] private IntoxicationSettings barkinzIntoxication;

    public void ClearData()
    {
        LoadSettingsFromInfo = false;
        currencyOwned = 0;
        individualSettings = new WorldTileSettings();
        barkinzIntoxication = new IntoxicationSettings();
    }

    public void SetIntoxicationData(ActivePlayer player)
    {
        barkinzIntoxication.InitializeSettingsAfterTime();
        player.ActiveSessionIntoxication = barkinzIntoxication;
    }

    public void UpdateIntoxicationSettings(ActivePlayer player)
    {
        barkinzIntoxication = player.ActiveSessionIntoxication;
    }

    public void SetPlayerPosition(ActivePlayer p)
    {
        p.transform.position = individualSettings.PlayerTile.centerPosition;
    }

    public void SetWorldTileFromSettings(WorldTile toSet)
    {
        individualSettings.GenerateTiles(toSet);
        individualSettings.InitializeWorld(toSet);
    }

    public void UpdateWorldTileSettings(WorldTile world)
    {
        individualSettings = new WorldTileSettings(world);
    }
}

[Serializable]
public class IntoxicationSettings
{
    public float currentTab;
    public float intoxicationLevel;
    DateTime lastDrinkTaken;
    float maxIntoxication = 100;

    public bool TabPaid { get => currentTab >= 0; }

    public IntoxicationSettings() { intoxicationLevel = 0; }
    public IntoxicationSettings(ActivePlayer ap)
    {
        intoxicationLevel = ap.ActiveSessionIntoxication.intoxicationLevel;
    }

    public void SoberUp()
    {
        if (intoxicationLevel > 0)
        {
            intoxicationLevel -= Time.deltaTime * Mathf.Ceil(intoxicationLevel) / maxIntoxication;
        }
    }

    public void InitializeSettingsAfterTime()
    {
        DateTime activeTime = DateTime.Now;
        double alcWithdrawal = (activeTime - lastDrinkTaken).TotalSeconds;
        float intl = intoxicationLevel;
        intoxicationLevel = intl - (float)alcWithdrawal;
        Debug.LogFormat("Setting Intoxication Level  from {0} to {1}", intl, intoxicationLevel);
    }

    public void Intoxicate(float chug, Drink current)
    {
        intoxicationLevel += current.drinkStrength / 15 * chug;
    }

    public void DrinkTaken()
    {
        lastDrinkTaken = DateTime.Now;
    }
}

