﻿using System.Collections;
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
    public bool hasAlcoholPoisoning { get; private set; }
    public float soberRate;
    public float soberingBuffer = 10f;

    public IntoxicationSettings() { intoxicationLevel = 0; }
    public IntoxicationSettings(ActivePlayer ap)
    {
        intoxicationLevel = ap.ActiveSessionIntoxication.intoxicationLevel;
    }

    public void SoberUp()
    {
        soberRate = hasAlcoholPoisoning ? 5: 2;
        soberingBuffer -= Time.deltaTime;
        if (intoxicationLevel > 0 && soberingBuffer < 0)
        {
            intoxicationLevel -= (Time.deltaTime/soberRate) * Mathf.Ceil(intoxicationLevel) / maxIntoxication;
        }

        if(hasAlcoholPoisoning && intoxicationLevel < 3) { hasAlcoholPoisoning = false; }
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
        soberingBuffer = 10f;
        if (current.drinkName != DrinkMenu.Water.drinkName)
        {
            intoxicationLevel += current.drinkStrength / 15 * chug;
            if (!hasAlcoholPoisoning && intoxicationLevel > 97) { hasAlcoholPoisoning = true; }
        } else
        {
            intoxicationLevel += current.drinkStrength / 20 * chug;
        }
    }

    public void DrinkTaken()
    {
        lastDrinkTaken = DateTime.Now;
    }
}

