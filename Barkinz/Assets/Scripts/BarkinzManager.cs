using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class BarkinzManager : MonoBehaviour
{
    private static BarkinzManager barkinz;
    public static Dictionary<string, BarkinzInfo> BarkinzCodeLookup { get => "BarkinzInfo".CreateLookup(); }
    public static BarkinzInfo PrimaryBarkinz;

    const string gameplaySceneName = "GameScene";
    public static string introductionDialoguePath { get; private set; }

    private void Awake()
    {
        if(barkinz == null) { barkinz = this; } else { Destroy(this); }
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoad;
        introductionDialoguePath = Application.streamingAssetsPath + "/TutorialDialogue.json";
    }

    public static event Action<BarkinzInfo> InitializeBarkinzData;

    void OnSceneLoad(Scene s, LoadSceneMode mode)
    {
        if (s.name == gameplaySceneName)
        {
            InitializeBarkinzData(PrimaryBarkinz);
            Debug.Log("Initializing " + PrimaryBarkinz.name + " as Primary Barkinz");
        }
    }

    private void OnApplicationQuit()
    {
        if(SceneManager.GetActiveScene().name == gameplaySceneName) { PrimaryBarkinz.LoadSettingsFromInfo = true; }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

}

public static class BarkinzMethods
{
    public static Dictionary<string, BarkinzInfo> CreateLookup(this string resourcePath)
    {
        BarkinzInfo[] infos = Resources.LoadAll<BarkinzInfo>(resourcePath);
        Dictionary<string, BarkinzInfo> keyValuePairs = new Dictionary<string, BarkinzInfo>();
        foreach(BarkinzInfo i in infos)
        {
            if (!keyValuePairs.ContainsValue(i))
            {
                keyValuePairs.Add(i.BarkinzCode, i);
            }
        }
        return keyValuePairs;
    }
}