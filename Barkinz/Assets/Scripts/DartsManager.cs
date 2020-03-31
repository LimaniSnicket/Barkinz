using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DartsManager : MonoBehaviour, IGameMode
{
    private DartsManager darts;
    public GameObject DartPrefab;
    public GameObject Dartboard;
    public Transform CameraPosition;
    public int DartsOnBegin;
    public int DartsRemaining;
    bool CanSpawnDart { get => DartsRemaining > 0; }

    public Dart ActiveDart;
    public float ShakeFactor = 0.1f;
    public float ShakeAmount = 0.2f;
    public Color colorHit;

    public static DartGame ActiveDartGame;
    public ActivePlayer activePlayer;

    float OutOfBounds { get => Dartboard.transform.position.y - 15; }
    public ActiveGameFunction GameModeFunction { get => ActiveGameFunction.DARTS; }

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
    }

    private void Update()
    {
        transform.position += UnityEngine.Random.insideUnitSphere * ShakeFactor * Mathf.Sin(ShakeAmount) ;
        if(ActiveDart != null)
        {
            if (ActiveDart.transform.position.y < OutOfBounds) { Destroy(ActiveDart.gameObject); CreateDart(); ActiveDartGame.DartMiss(); }
        }
    }

    void OnDartHit(Color contactColor, int pointValue)
    {
        colorHit = contactColor;
        if (contactColor == Color.red) { Debug.Log("red"); }
        CreateDart();
        if (ActiveDartGame != null)
        {
            ActiveDartGame.RegisterPointChanges(pointValue);
        }
    }

    void InitializeDartGame()
    {
        ActiveDartGame = new DartGame(activePlayer);
        CameraMovement.AlignWithTransform(CameraPosition, false);
        DartsRemaining = DartsOnBegin;
        CreateDart();
    }

    void CreateDart()
    {
        if (CanSpawnDart)
        {
            ActiveDart = Instantiate(DartPrefab, transform).GetComponent<Dart>();
            ActiveDart.SetPlayerToTrack(activePlayer ?? null);
            DartsRemaining--;
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
        ActiveDartGame = null;
        MinigameManager.ExitMode();
    }

    private void OnDestroy()
    {
        Dart.DartHit -= OnDartHit;
        DartGame.GameComplete -= OnGameComplete;
        MinigameManager.EnteredMode -= OnModeChange;
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

    public delegate void BroadcastGameComplete();
    public static event BroadcastGameComplete GameComplete;

    public DartGame(ActivePlayer player)
    {
        StartingPointTotal = 301;
        currentPoints = 0;
        DartsRemaining = DartsOnStart;
        p = player;
    }

    public void RegisterPointChanges(int pointChange)
    {
        currentPoints += pointChange;
        DartsRemaining--;
        Debug.Log(currentPoints);
        if (currentPoints >= StartingPointTotal || DartsRemaining == 0)
        {
            GameComplete();
            RunEndGame();
        }

    }

    public void DartMiss()
    {
        DartsRemaining--;
        if (DartsRemaining == 0)
        {
            GameComplete();
            RunEndGame();
        }
    }

    void RunEndGame()
    {
        MinigameManager.activeCurrency += ConvertPointsToCurrency();
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
