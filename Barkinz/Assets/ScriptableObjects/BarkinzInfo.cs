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
    [SerializeField] private InventorySettings barkinzInventory;
    public List<ActiveGameFunction> playedModes;

    public BarkinzData barkinzData;

    public void ClearData()
    {
        LoadSettingsFromInfo = false;
        currencyOwned = 0;
        individualSettings = new WorldTileSettings();
        barkinzIntoxication = new IntoxicationSettings();
        barkinzData = new BarkinzData();
    }

    public void SetIntoxicationData(ActivePlayer player)
    {
        barkinzIntoxication.InitializeSettingsAfterTime();
        player.ActiveSessionIntoxication = barkinzIntoxication;
    }

    public void UpdateIntoxicationSettings(ActivePlayer player)
    {
        barkinzIntoxication = player.ActiveSessionIntoxication;
        currencyOwned = MinigameManager.activeCurrency;
    }

    public void SetPlayerPosition(ActivePlayer p)
    {
        p.transform.position = individualSettings.StartTile.centerPosition;
    }

    public void SetWorldTileFromSettings(WorldTile toSet)
    {
        //individualSettings.GenerateTiles(toSet);
        PopulateWorld(toSet);
        individualSettings.InitializeWorld(toSet);
    }

    public void UpdateWorldTileSettings(WorldTile world)
    {
        individualSettings = new WorldTileSettings(world);
        barkinzData = new BarkinzData(world);
    }

    public void UpdateInventorySettings(InventorySettings inventory)
    {
        barkinzInventory = inventory;
    }

    void PopulateWorld(WorldTile toPopulate)
    {
        if(barkinzData == null || barkinzData.worldTileSettings == null || barkinzData.worldTileSettings.Count <= 0)
        {
            return;
        } else
        {
            foreach (TileData t in barkinzData.worldTileSettings)
            {
                Tile tile = new Tile(t);
                tile.InitializeTile(toPopulate.TileSprite, toPopulate);
                toPopulate.Tiles.Add(tile);
                toPopulate.GridPositions[tile.GridPosition.x, tile.GridPosition.y] = tile;
                toPopulate.TileLookup.Add(tile.GetGameObject(), tile);
                if (tile.isStartingTile) { toPopulate.StartTile = tile; }
            }
        }
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
    string[] intoxicationMessages { get=> new string[] { "Stone-Cold Sober", "Tipsy", "Lit", "Smacked", "Absolutely Trashed"};}
    public string intoxicationMessage { get { return intoxicationMessages[IntoxicationRange]; } }

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
        if (current.drinkName != "Water")
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

    public int IntoxicationRange
    {
        get
        {
            if(intoxicationLevel < 10) { return 0; }
            if(intoxicationLevel.SqueezeFloats(10, 20)) { return 1; }
            if (intoxicationLevel.SqueezeFloats(20, 45)) { return 2; }
            if (intoxicationLevel.SqueezeFloats(45, 75)) { return 3; }
            return 4;
        }
    }
}

