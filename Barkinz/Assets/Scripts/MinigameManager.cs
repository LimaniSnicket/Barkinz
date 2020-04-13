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
    public Image TextBG;


    public DialogueReader dialogueReader;

    public static bool SwappingMode { get; private set; }
    public static bool AcceptPlayerInput { get; private set; }

    private static Stack<ActiveGameFunction> gameFunctionBacklog;

    private void Awake()
    {
        BarkinzManager.InitializeBarkinzData += SetCurrencyOnInitialize;
        Bartender.DrinkOnTab += OnDrinkTab;
        ActivePlayer.EnteredTaggedArea += OnEnteredTaggedArea;
        ActivePlayer.EnteredTaggedAreaWithDialogue += OnEnteredTaggedDialogueArea;
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
        } else
        {
            AcceptPlayerInput = true;
        }
        gameFunctionBacklog = new Stack<ActiveGameFunction>();
    }

    public static event Action<ActiveGameFunction> EnteredMode;

    public void Update()
    {
        BarCanvas.SetActive(ValidMode(ActiveGameFunction.BAR));
        TriviaCanvas.SetActive(ValidMode(ActiveGameFunction.TRIVIA));
        dialogueReader.MaintainDialogueDisplay();
        AcceptPlayerInput = ValidMode(ActiveGameFunction.NONE);

        if (Input.GetKeyDown(KeyCode.R) && activeGameFunction == ActiveGameFunction.NONE)
        {
            activeGameFunction = ActiveGameFunction.SHOP;
            EnteredMode(ActiveGameFunction.SHOP);
        }

        TextBG.gameObject.SetActive(ValidMode(ActiveGameFunction.DIALOGUE));

        if (CanEnterMode())
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(EnterMode(toEnter));
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitMode();
        }
    }

    public static void EnterGameMode(ActiveGameFunction toEnter)
    {
        if (CanEnterMode()) { gameManager.StartCoroutine(gameManager.EnterMode(toEnter)); }
    }

    public static void ExitMode()
    {
        gameManager.StartCoroutine(gameManager.EnterMode(ActiveGameFunction.NONE));
        CameraMovement.AlignWithTransform();
        CameraMovement.ResetCameraZoom();
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

    public static bool CanEnterMode(bool checkBacklog = false)
    {
        if (checkBacklog) { return !SwappingMode && gameFunctionBacklog.Count <= 0; }
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

    void OnEnteredTaggedDialogueArea(string t, string dialoguePath)
    {
        if (GameFunctionToTagLookup.ContainsKey(t)) { gameFunctionBacklog.Push(GameFunctionToTagLookup[t]); }
        gameFunctionBacklog.Push(ActiveGameFunction.DIALOGUE);
        dialogueReader.InitializeDialogue(dialoguePath, this);
        StartCoroutine(EnterMode(ActiveGameFunction.DIALOGUE));
    }

    void OnDialogueComplete()
    {
        StartCoroutine(EnterMode(ActiveGameFunction.NONE, MeerkatMac));
        AcceptPlayerInput = true;
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

    private void OnApplicationQuit()
    {
        BarkinzManager.PrimaryBarkinz.currencyOwned = activeCurrency;
    }

    private void OnDestroy()
    {
        BarkinzManager.InitializeBarkinzData -= SetCurrencyOnInitialize;
        Bartender.DrinkOnTab -= OnDrinkTab;
        ActivePlayer.EnteredTaggedArea -= OnEnteredTaggedArea;
        ActivePlayer.EnteredTaggedAreaWithDialogue -= OnEnteredTaggedDialogueArea;
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
    DIALOGUE = 5,
    FOCUS = 6
}

public interface IGameMode
{
    ActiveGameFunction GameModeFunction { get; }
    void OnModeChange(ActiveGameFunction entered);
}

[Serializable]
public class DialogueReader 
{
    public TextMeshProUGUI dialogueTextMesh;
    public DialogueNode activeDialogueNode;
    public DialogueTree treeToRead;
    public ActiveGameFunction previousGameFunction { get; private set; }
    public bool onFinalNode { get => activeDialogueNode != null && activeDialogueNode.pointer < 0; }
    public string activeLine, displayLine;
    public bool reading { get; private set; }

    private Dictionary<string, DialogueTree> ArchivedDialogue;

    public DialogueReader() { activeDialogueNode = new DialogueNode(); ArchivedDialogue = new Dictionary<string, DialogueTree>(); }
    public DialogueReader(TextMeshProUGUI textMesh) { dialogueTextMesh = textMesh; activeDialogueNode = new DialogueNode(); ArchivedDialogue = new Dictionary<string, DialogueTree>(); }

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
        bool w = false;
        while (l.Count > 0)
        {
            if (!w) { w = true; mb.StartCoroutine(WriteLine(new Queue<char>(l.Peek().ToCharArray()), 0.0001f)); }
            //activeLine = l.Peek();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!writingLine) { l.Dequeue(); w = false; } else { mb.StopCoroutine("WriteLine"); activeLine = l.Peek();writingLine = false; }
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

    bool writingLine;

    private IEnumerator WriteLine(Queue<char> c, float interval)
    {
        activeLine = "";
        string affector = "";
        writingLine = true;
        while (c.Count > 0 && writingLine)
        {
            if (c.Peek() == '<')
            {
                affector = "";
                while (!affector.EndsWith(">"))
                {
                    if (c.Peek() == '>')
                    {
                        affector += c.Peek();
                    } else
                    {
                        affector += c.Dequeue();
                    }
                }
            }
            if (c.Peek() == '>')
            {
                c.Dequeue();
            } else
            {
                activeLine += affector + c.Dequeue();
            }
          
            yield return new WaitForSeconds(interval);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                yield return new WaitForSeconds(interval / 2);
                writingLine = false;
                break;
            }
            yield return null;
        }
        writingLine = false;
    }

}

public class DialogueTree
{
    public int startingIndex;
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

