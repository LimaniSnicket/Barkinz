using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DartsManager : MonoBehaviour, IGameMode, IComparer<ScoreData>
{
    private DartsManager darts;
    public GameObject DartPrefab;
    public GameObject Dartboard;
    public Transform CameraPosition;

    public Dart ActiveDart;
    public float ShakeFactor = 0.1f;
    public float ShakeAmount = 0.2f;
    public Color colorHit;
    public TextMeshPro activeText, highText;

    public static DartGame ActiveDartGame;
    public ActivePlayer activePlayer;

    float OutOfBounds { get => Dartboard.transform.position.y - 15; }
    public ActiveGameFunction GameModeFunction { get => ActiveGameFunction.DARTS; }

    public static ScoreData highscoreInfo;

    private void Awake()
    {
        if(darts == null) { darts = this; } else { Destroy(this); }
        Dart.DartHit += OnDartHit;
        DartGame.GameComplete += OnGameComplete;
        MinigameManager.EnteredMode += OnModeChange;
    }

    private void Start()
    {
        colorHit = new Color();
        try
        {
            highscoreInfo = UserDataStorage.activeUserData.GetScore("DARTS");
        }
        catch (NullReferenceException) { highscoreInfo = new ScoreData(0, "No High Score!"); }
    }

    private void Update()
    {
        transform.position += UnityEngine.Random.insideUnitSphere * ShakeFactor * Mathf.Sin(ShakeAmount) ;
        if(ActiveDart != null)
        {
            if (ActiveDart.transform.position.y < OutOfBounds) { Destroy(ActiveDart.gameObject); CreateDart(); ActiveDartGame.DartMiss(); }
        }

        if (ActiveDartGame != null)
        {
            ActiveDartGame.infoDisplay.text = ActiveDartGame.PointsDisplay(true);
        }
    }

    void OnDartHit(Color contactColor, int pointValue)
    {
        colorHit = contactColor;
        int p = pointValue;
        if (contactColor == Color.red) { Debug.Log("red"); p = pointValue * 2; }
        if(contactColor == Color.green) { p = pointValue * 3; }
        CreateDart();
        if (ActiveDartGame != null)
        {
            ActiveDartGame.RegisterPointChanges(p);
        }
    }

    void InitializeDartGame()
    {
        ActiveDartGame = new DartGame(activePlayer, activeText);
        CameraMovement.AlignWithTransform(CameraPosition, false);
        CreateDart();
    }

    void CreateDart()
    {
        if (ActiveDartGame != null && ActiveDartGame.CanSpawnDart)
        {
            ActiveDart = Instantiate(DartPrefab, transform).GetComponent<Dart>();
            ActiveDart.SetPlayerToTrack(activePlayer ?? null);
        }
    }

    public void OnModeChange(ActiveGameFunction entered)
    {
        if (entered == GameModeFunction)
        {
            InitializeDartGame();
        }
    }

    void OnGameComplete()
    {
        if (ActiveDart != null) { Destroy(ActiveDart); }
        if (Compare(ActiveDartGame.gameScoreData, highscoreInfo) > 0)
        {
            highscoreInfo = ActiveDartGame.gameScoreData;
        }
        ActiveDartGame = null;
        MinigameManager.ExitMode();
    }

    private void OnDestroy()
    {
        Dart.DartHit -= OnDartHit;
        DartGame.GameComplete -= OnGameComplete;
        MinigameManager.EnteredMode -= OnModeChange;
    }

    public int Compare(ScoreData x, ScoreData y)
    {
        if (x.score <=0 && y.score <= 0) { return 0; }
        if(x.score == y.score) { return 0; }
        if(x.score > y.score) { return -1; }
        return 1;
    }
}

[Serializable]
public class DartGame
{
    int DartsOnStart = 6;
    protected int StartingPointTotal;
    private int currentPoints;
    public int DartsRemaining;
    ActivePlayer p;
    public TextMeshPro infoDisplay;
    public ScoreData gameScoreData;

    public bool CanSpawnDart { get => DartsRemaining > 0; }

    public delegate void BroadcastGameComplete();
    public static event BroadcastGameComplete GameComplete;

    public DartGame(ActivePlayer player, TextMeshPro i)
    {
        StartingPointTotal = 301;
        currentPoints = 0;
        DartsRemaining = DartsOnStart;
        p = player;
        infoDisplay = i;
    }

    public void RegisterPointChanges(int pointChange)
    {
        currentPoints += pointChange;
        DartsRemaining--;
        if (currentPoints >= StartingPointTotal || DartsRemaining == 0)
        {
            RunEndGame();
        }
    }

    public string PointsDisplay(bool active)
    {
        string header = active ? "Current Score:" : "All Time Highs:";
        string points =  CurrentPointTotal().ToString();
        string darts = "Darts To Go: " + DartsRemaining.ToString();
        return header + '\n' + points + '\n' + darts;
    }

    public void DartMiss()
    {
        DartsRemaining--;
        if (DartsRemaining == 0)
        {
            RunEndGame();
        }
    }

    void RunEndGame()
    {
        MinigameManager.activeCurrency += ConvertPointsToCurrency();
        gameScoreData = new ScoreData((int)CurrentPointTotal(), BarkinzManager.PrimaryBarkinz.BarkinzType);
        GameComplete();
    }

    float ConvertPointsToCurrency()
    {
        float intoxicationMod = p.ActiveSessionIntoxication.intoxicationLevel/10f;
        float percentTotal = currentPoints / StartingPointTotal;

        if (percentTotal == 1f) { return currentPoints  * intoxicationMod; }
        if (percentTotal > 1f)
        {
            return currentPoints / 1.5f * intoxicationMod;
        }
        return (currentPoints / 1.5f * percentTotal) * intoxicationMod;
    }

    public float CurrentPointTotal()
    {
        return StartingPointTotal - currentPoints;
    }
}
