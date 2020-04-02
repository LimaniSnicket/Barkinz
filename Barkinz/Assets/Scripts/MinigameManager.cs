using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class MinigameManager : MonoBehaviour
{
    private static MinigameManager gameManager;
    public ActiveGameFunction activeGameFunction;

    public static float activeCurrency;
    private const float startingAmount = 50f;

    public GameObject BarCanvas, TriviaCanvas;
    public UtilityCharacterInformation MeerkatMac;
    Dictionary<string, ActiveGameFunction> GameFunctionToTagLookup;
    ActiveGameFunction toEnter;
    ActivePlayer player;
    public TextMeshProUGUI dialogueTextMesh;


    public DialogueReader dialogueReader;

    public static bool SwappingMode { get; private set; }
    public static bool AcceptPlayerInput { get; private set; }

    private void Awake()
    {
        BarkinzManager.InitializeBarkinzData += SetCurrencyOnInitialize;
        Bartender.DrinkOnTab += OnDrinkTab;
        ActivePlayer.EnteredTaggedArea += OnEnteredTaggedArea;
        ActivePlayer.SetActivePlayer += OnActivePlayerSet;
        DialogueReader.OnDialogueCompleted += OnDialogueComplete;
    }

    private void Start()
    {
        if(gameManager == null) { gameManager = this; } else { Destroy(this); }
        activeGameFunction = 0;
        BarCanvas.gameObject.SetActive(false);
        TriviaCanvas.SetActive(false);
        GameFunctionToTagLookup = new Dictionary<string, ActiveGameFunction>();
        GameFunctionToTagLookup.Add("Bar", ActiveGameFunction.BAR);
        GameFunctionToTagLookup.Add("Trivia",ActiveGameFunction.TRIVIA);
        GameFunctionToTagLookup.Add("Darts", ActiveGameFunction.DARTS);
        dialogueReader = new DialogueReader(dialogueTextMesh);
        if (!BarkinzManager.PrimaryBarkinz.LoadSettingsFromInfo) {
            StartCoroutine(EnterMode(ActiveGameFunction.DIALOGUE, MeerkatMac));
            dialogueReader.InitializeDialogue(BarkinzManager.introductionDialoguePath, this, (IZoomOn)MeerkatMac);
        }
    }

    public static event Action<ActiveGameFunction> EnteredMode;

    public void Update()
    {
        BarCanvas.SetActive(ValidMode(ActiveGameFunction.BAR));
        TriviaCanvas.SetActive(ValidMode(ActiveGameFunction.TRIVIA));
        dialogueReader.MaintainDialogueDisplay();

        if (Input.GetKeyDown(KeyCode.T) && activeGameFunction == ActiveGameFunction.NONE)
        {
            activeGameFunction = ActiveGameFunction.TRIVIA;
        }

        if (Input.GetKeyDown(KeyCode.P) && activeGameFunction == ActiveGameFunction.NONE)
        {
            activeGameFunction = ActiveGameFunction.SHOP;
            EnteredMode(ActiveGameFunction.SHOP);
        }

        if (CanEnterMode())
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                StartCoroutine(EnterMode(toEnter));
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitMode();
        }
    }

    public static void ExitMode()
    {
        gameManager.StartCoroutine(gameManager.EnterMode(ActiveGameFunction.NONE));
        EnteredMode(ActiveGameFunction.NONE);
        CameraMovement.AlignWithTransform();
    }

    void OnActivePlayerSet(ActivePlayer p)
    {
        player = p;
    }

    public static void RewardPlayer(float amount)
    {
        activeCurrency += Mathf.Round(amount);
    }

    void OnDrinkTab(float t)
    {
        activeCurrency -= t;
    }

    public static bool CanEnterMode()
    {
        return !SwappingMode && ValidMode(ActiveGameFunction.NONE);
    }

    public static bool ValidMode(ActiveGameFunction gameFunction)
    {
        return gameManager.activeGameFunction == gameFunction;
    }

    void SetCurrencyOnInitialize(BarkinzInfo b)
    {
        if (b.LoadSettingsFromInfo)
        {
            activeCurrency = b.currencyOwned;
        } else
        {
            activeCurrency = startingAmount;
        }
    }

    void OnEnteredTaggedArea(string t)
    {
        if (GameFunctionToTagLookup.ContainsKey(t))
        {
            toEnter = GameFunctionToTagLookup[t];
        }
    }

    void OnDialogueComplete()
    {
        StartCoroutine(EnterMode(ActiveGameFunction.NONE, MeerkatMac));
        CameraMovement.ResetCameraZoom();
    }

    public IEnumerator EnterMode(ActiveGameFunction toEnter)
    {
        SwappingMode = true;
        if (toEnter == ActiveGameFunction.BAR) { yield return StartCoroutine(MacPop(true, 3f)); }
        Debug.Log("Swapping Modes");
        yield return new WaitForSeconds(1);
        ActiveGameFunction previous = activeGameFunction;
        activeGameFunction = toEnter;
        if (previous == ActiveGameFunction.BAR) { yield return StartCoroutine(MacPop(false, 5f)); }
        EnteredMode(activeGameFunction);
        Debug.Log("Mode Swapped");
        SwappingMode = false;
    }

    public IEnumerator EnterMode(ActiveGameFunction toEnter, UtilityCharacterInformation utilityChar)
    {
        SwappingMode = true;
        if (utilityChar.name == "Meerkat Mac" && toEnter != ActiveGameFunction.NONE) { yield return StartCoroutine(MacPop(true, 3f, false)); }
        yield return new WaitForSeconds(0.1f);
        ActiveGameFunction prev = activeGameFunction;
        activeGameFunction = toEnter;
        if (utilityChar.name == "Meerkat Mac" && toEnter == ActiveGameFunction.NONE) { yield return StartCoroutine(MacPop(false, 5f, false)); }
        EnteredMode(activeGameFunction);
        SwappingMode = false;
    }

    IEnumerator MacPop(bool poppingUp, float speed, bool usePlayerPosition = true)
    {
        float xAxisStart = usePlayerPosition ? player.transform.position.x : MeerkatMac.StartPos.x;
        float xAxisEnd = usePlayerPosition ? player.transform.position.x : MeerkatMac.EndPos.x;
        Vector3 start = poppingUp ? new Vector3(xAxisStart, MeerkatMac.StartPos.y, MeerkatMac.StartPos.z) : new Vector3(xAxisEnd, MeerkatMac.EndPos.y, MeerkatMac.EndPos.z);
        Vector3 goTo = poppingUp ? new Vector3(xAxisEnd, MeerkatMac.EndPos.y, MeerkatMac.EndPos.z) : new Vector3(xAxisStart,MeerkatMac.StartPos.y, MeerkatMac.StartPos.z);
        MeerkatMac.OverworldCharacter.transform.position = start;
        while (!MeerkatMac.OverworldCharacter.transform.position.SqueezeVectors(goTo))
        {
            MeerkatMac.OverworldCharacter.transform.position = Vector3.Lerp(MeerkatMac.OverworldCharacter.transform.position, goTo, Time.deltaTime * speed);
            yield return null;
        }
        MeerkatMac.OverworldCharacter.transform.position = goTo;
    }

    private void OnDestroy()
    {
        BarkinzManager.InitializeBarkinzData -= SetCurrencyOnInitialize;
        Bartender.DrinkOnTab -= OnDrinkTab;
        ActivePlayer.EnteredTaggedArea -= OnEnteredTaggedArea;
        ActivePlayer.SetActivePlayer -= OnActivePlayerSet;
        DialogueReader.OnDialogueCompleted -= OnDialogueComplete;
    }
}

[System.Serializable]
public struct UtilityCharacterInformation : IZoomOn
{
    public string name;
    public GameObject OverworldCharacter;
    public Vector3 StartPos, EndPos;

    public float CameraOrthoSize { get => 1; }
    public Transform ZoomObjectTransform { get => OverworldCharacter.transform; }

    public void SetPosition(Vector3 pos)
    {
        OverworldCharacter.transform.position = pos;
    }

    public Vector3 ZoomCamPosition()
    {
        Vector3 p = EndPos + new Vector3(-1, 1, -1);
        return p;
    }
}

public enum ActiveGameFunction
{
    NONE = 0,
    SHOP = 1,
    BAR = 2,
    DARTS = 3,
    TRIVIA = 4,
    DIALOGUE = 5
}

public interface IGameMode
{
    ActiveGameFunction GameModeFunction { get; }
    void OnModeChange(ActiveGameFunction entered);
}

[System.Serializable]
public class DialogueReader 
{
    public TextMeshProUGUI dialogueTextMesh;
    public DialogueNode activeDialogueNode;
    public DialogueTree treeToRead;
    public ActiveGameFunction previousGameFunction { get; private set; }
    public bool onFinalNode { get => activeDialogueNode != null && activeDialogueNode.pointer < 0; }
    public string activeLine;
    public bool reading { get; private set; }
    public DialogueReader() { activeDialogueNode = new DialogueNode(); }
    public DialogueReader(TextMeshProUGUI textMesh) { dialogueTextMesh = textMesh; }

    public delegate void DialogueComplete();
    public static event DialogueComplete OnDialogueCompleted;

    public void InitializeDialogue(string path, MonoBehaviour mb)
    {
        string json = File.ReadAllText(path);
        treeToRead = JsonUtility.FromJson<DialogueTree>(json);
        activeDialogueNode = treeToRead.nodesToRead[0];
        if (!reading) { mb.StartCoroutine(ReadDialogue(treeToRead.nodesToRead, 0, mb)); }
    }

    public void InitializeDialogue(string path, MonoBehaviour mb, IZoomOn zoomOn)
    {
        CameraMovement.ZoomOn(zoomOn);
        InitializeDialogue(path, mb);
    }

    public void MaintainDialogueDisplay()
    {
        if (!MinigameManager.ValidMode(ActiveGameFunction.DIALOGUE)) { dialogueTextMesh.text = ""; } else
        {
            dialogueTextMesh.text = activeLine;
        }
    }

     private IEnumerator ReadDialogue(List<DialogueNode> nodes, int pointer, MonoBehaviour mb)
    {
        if(pointer >=nodes.Count || nodes[pointer] == null || nodes[pointer].lines.Length <= 0) { yield return null; }
        reading = true;
        activeDialogueNode = nodes[pointer];
        Queue<string> l = new Queue<string>(nodes[pointer].lines);
        while (l.Count > 0)
        {
            activeLine = l.Peek();
            Debug.Log(activeLine);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                l.Dequeue();
            }
            yield return null;
        }
        if (onFinalNode)
        {
            reading = false;
            Debug.Log("Exiting Dialogue");
            OnDialogueCompleted();
        }
        else
        {
            mb.StartCoroutine(ReadDialogue(nodes, activeDialogueNode.pointer, mb));
        }
    }

}

public class DialogueTree
{
    public List<DialogueNode> nodesToRead;
}

[Serializable]
public class DialogueNode
{
    public int ID;
    public string speaker;
    public string[] lines;
    public int pointer;
    public DialogueNode() { }
    public DialogueNode(int id, string sp, string[] l, int p)
    {
        ID = id;
        speaker = sp;
        lines = new string[l.Length];
        Array.Copy(l, lines, l.Length);
        pointer = p;
    }
}

