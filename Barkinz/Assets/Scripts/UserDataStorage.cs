using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using BayatGames.SaveGameFree;

public class UserDataStorage : MonoBehaviour
{
    private static UserDataStorage userData;
    public static UserData activeUserData { get; private set; }
    private static Dictionary<string, UserData> userLookup;
    static string tempUserGen;
    public string activeUserKey;

    const string userDNE = "User does not exist in Database";

    private void Awake()
    {
        if(userData == null) { userData = this; } else { Destroy(this); }
        DontDestroyOnLoad(this);
        LoadUserDataBaseFromSaveData();
    }

    public static bool ValidateUser(string key, bool newUser = false)
    {
        if (!newUser) {
            if (userLookup == null || userLookup.Count <= 0) { return false; }
            return userLookup.ContainsKey(key);
        }

        return string.Compare(key, tempUserGen) == 0;
    }

    public static Dictionary<string, UserData> lookup { get => userLookup; }

    public static bool NewUser(string key) { return !ValidateUser(key); }

    public static void SetTempUserKey(string key) { tempUserGen = key; }

    public static void AddUser(string name)
    {
        if(userLookup == null) {
            Debug.Log("User Database is null. Initializing now for posterity. You're welcome Vera from 3 seconds into the future.");
            userLookup = new Dictionary<string, UserData>();
            AddUser(name);
        } else
        {
            userLookup.Add(name, new UserData());
            Debug.LogFormat("Adding new user: {0} to Database. Welcome to hell motherfucker.", name);
        }
    }

    public static void SetActiveUserKey(string s)
    {
        userData.activeUserKey = s;
    }

    public static void LoadUserData(string key)
    {
        if (userLookup == null || userLookup.Count<=0) { Debug.Log(userDNE); } else {
            try
            {
                activeUserData = userLookup[key];
            }
            catch (KeyNotFoundException) { Debug.Log(userDNE); }
        }
    }

    static void LoadUserDataBaseFromSaveData(bool fromWeb = false)
    {
        if (!fromWeb) {
            try
            {
                userLookup = SaveGame.Load<Dictionary<string, UserData>>("userDataBase");
                Debug.Log(userLookup.Count);
            }
            catch (NullReferenceException) {
                userLookup = new Dictionary<string, UserData>();
                Debug.Log("New Database Created");
            }
        }
        else
        {

        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Serialize UserData here!");
        SaveGame.Save<Dictionary<string, UserData>>("userDataBase", userLookup);
    }

}

[Serializable]
public class UserData
{
    Dictionary<string, BarkinzData> userBarkinzLookup;
    public BarkinzData activeBarkinzData { get; private set; }
    public UserData()
    {
        userBarkinzLookup = new Dictionary<string, BarkinzData>();
    }

    public int barkinzRedeemed {
        get
        {
            if(userBarkinzLookup == null) { return -1; }
            return userBarkinzLookup.Count;
        }
    }

    bool barkinzOwned(string lookup)
    {
        if (userBarkinzLookup == null || userBarkinzLookup.Count <=0) { return false; }
        return userBarkinzLookup.ContainsKey(lookup);
    }

    public void RedeemBarkinz(string lookup)
    {
        if (!barkinzOwned(lookup)) { userBarkinzLookup.Add(lookup, new BarkinzData()); }
        activeBarkinzData = userBarkinzLookup[lookup];
    }
}

[Serializable]
public class BarkinzData
{
    public string barkinzLookup;
    public bool loadSettingsFromPreviousSession;
    public float balance;
    public List<TileData> worldTileSettings;
    public BarkinzData() { }
    public BarkinzData(WorldTile w)
    {
        worldTileSettings = new List<TileData>();
        for (int i = 0; i< w.Tiles.Count; i++)
        {
            worldTileSettings.Add(new TileData(w.Tiles[i]));
        }
    }
}

