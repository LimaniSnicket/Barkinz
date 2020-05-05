using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class BarkinzManager : MonoBehaviour
{
    private static BarkinzManager barkinz;
    public static Dictionary<string, BarkinzInfo> BarkinzCodeLookup { get => "BarkinzInfo".CreateLookup(); }
    public static BarkinzInfo PrimaryBarkinz;
    public BarkinzInfo defaultBarkinz;
    public GameObject inactiveBarkinzPrefab;
    public static List<NpcController> npcBarkinz;

    const string gameplaySceneName = "GameScene";
    public static string introductionDialoguePath { get; private set; }
    private bool enteredGameScene;

    public bool returnToMainMenu
    {
        get
        {
            if (SceneManager.GetActiveScene().name != gameplaySceneName) { return false; }
            if (!MinigameManager.ValidMode(ActiveGameFunction.NONE)) { return false; }
            return !CameraMovement.Zoomed;
        }
    }

    private void Awake()
    {
        if(barkinz == null) { barkinz = this; } else { Destroy(this); }
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoad;
        introductionDialoguePath = Application.streamingAssetsPath + "/TutorialDialogue.json";
        npcBarkinz = new List<NpcController>();
    }

    public static event Action<BarkinzInfo> InitializeBarkinzData;
    public delegate void QuittingGameScene();
    public static event QuittingGameScene OnGameSceneExit;

    void OnSceneLoad(Scene s, LoadSceneMode mode)
    {
        if (s.name == gameplaySceneName)
        {
            if(PrimaryBarkinz == null) { PrimaryBarkinz = defaultBarkinz; }
            InitializeBarkinzData(PrimaryBarkinz);
            Debug.Log("Initializing " + PrimaryBarkinz.name + " as Primary Barkinz");
            enteredGameScene = true;
            FaderBehavior.DoFade(1);
        } else
        {
            if (enteredGameScene) { PrimaryBarkinz.barkinzData.loadSettingsFromPreviousSession = true; }//PrimaryBarkinz.LoadSettingsFromInfo = true; }
        }
    }

    public static void GenerateNPC(Tile t)
    {
        BarkinzInfo b = BarkinzCodeLookup.GetRandomBarkinz(new List<string>() {PrimaryBarkinz.BarkinzCode });
        NpcController npc = Instantiate(barkinz.inactiveBarkinzPrefab).GetComponent<NpcController>();
        npc.SetBarkinz(b, t);
        npcBarkinz.Add(npc);
    }

    public static void AddToPlayedModes(ActiveGameFunction mode)
    {
        if (PrimaryBarkinz == null) { return; }
        if (!PrimaryBarkinz.playedModes.Contains(mode)) { PrimaryBarkinz.playedModes.Add(mode); Debug.LogFormat("Adding {0} to list of played modes", mode.ToString()); }
    }

    public static void OnClickQuitToMainMenu()
    {
        SceneManager.LoadScene(0);
        OnGameSceneExit();
    }

    private void OnApplicationQuit()
    {
        // if(SceneManager.GetActiveScene().name == gameplaySceneName) { PrimaryBarkinz.LoadSettingsFromInfo = true; }
        if (enteredGameScene) {
            PrimaryBarkinz.barkinzData.loadSettingsFromPreviousSession = true;
        }
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

    public static BarkinzInfo LookupBarkinz(this string barkinzName)
    {
        try
        {
            return Resources.Load<BarkinzInfo>("BarkinzInfo/" + barkinzName);
        } catch (NullReferenceException) { Debug.Log("Barkinz Not Found"); return null; }
    }

    public static BarkinzInfo GetRandomBarkinz(this Dictionary<string, BarkinzInfo> dict, List<string> exclude)
    {
        int i = UnityEngine.Random.Range(1, dict.Count);
        BarkinzInfo b = dict.ElementAt(i).Value;
        if (exclude.Contains(b.BarkinzCode)){ return dict.GetRandomBarkinz(exclude); }
        return b;
    }
}