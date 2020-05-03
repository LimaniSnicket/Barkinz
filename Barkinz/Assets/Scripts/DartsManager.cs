using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class DartsManager : MonoBehaviour, IGameMode, IComparer<ScoreData>
{
    private static DartsManager darts;
    public GameObject DartPrefab;
    public GameObject Dartboard;
    public Transform CameraPosition;

    public Dart ActiveDart;
    public float ShakeFactor = 0.1f;
    public float ShakeAmount = 0.2f;
    public Color colorHit;
    public TextMeshPro activeText, highText, wrapUp;
    public static DartGame ActiveDartGame;
    public ActivePlayer activePlayer;

    public GameObject numberEffect;
    Queue<NumberEffect> activeNumberEffects;

    float OutOfBounds { get => Dartboard.transform.position.y - 15; }
    public ActiveGameFunction GameModeFunction { get => ActiveGameFunction.DARTS; }

    public static ScoreData highscoreInfo;

    private void Awake()
    {
        if(darts == null) { darts = this; } else { Destroy(this); }
        Dart.DartHit += OnDartHit;
        DartGame.GameComplete += OnGameComplete;
        MinigameManager.EnteredMode += OnModeChange;
        HUD.ForfeitGameFunction += OnForfeit;
        HUD.OnClickSaveData += OnSave;
    }

    private void Start()
    {
        colorHit = new Color();
        try
        {
            highscoreInfo = UserDataStorage.activeUserData.GetScore("DARTS");
        }
        catch (NullReferenceException) { highscoreInfo = new ScoreData(0, "No High Score!"); }
        activeNumberEffects = new Queue<NumberEffect>();
    }

    private void Update()
    {
        if (!MinigameManager.ValidMode(GameModeFunction)){ return; }
        transform.position += UnityEngine.Random.insideUnitSphere * ShakeFactor * Mathf.Sin(ShakeAmount) ;
        if(ActiveDart != null)
        {
            if (ActiveDart.transform.position.y < OutOfBounds) { Destroy(ActiveDart.gameObject); CreateDart(); ActiveDartGame.DartMiss(); }
        }

        if (ActiveDartGame != null)
        {
            ActiveDartGame.infoDisplay.text = ActiveDartGame.PointsDisplay(true);
            highText.text = "All Time High:" + '\n' + highscoreInfo.scoreDisplay;
        }
        if(activeNumberEffects.Count > 0)
        {
            if (activeNumberEffects.Peek().fadedOut) {
                NumberEffect t = activeNumberEffects.Dequeue();
                Destroy(t.textmesh.gameObject);
            }
        }
    }

    void OnSave()
    {
        print("Saving the Darts Highscore Data");
        UserDataStorage.UpdateScore(highscoreInfo, "DARTS");
    }

    public static bool isActiveDart(Dart d) { return darts.ActiveDart == d; }

    void OnDartHit(Color contactColor, int pointValue, Vector3 pos)
    {
        colorHit = contactColor;
        int p = pointValue;
        if (contactColor == Color.red) { Debug.Log("red"); p = pointValue * 2; }
        if(contactColor == Color.green) { p = pointValue * 3; }
        NumberEffect(p , pos + new Vector3(0,0,1));
        CreateDart();
        if (ActiveDartGame != null)
        {
            ActiveDartGame.RegisterPointChanges(p);
        }
    }


    void NumberEffect(int s, Vector3 position)
    {
        GameObject g = Instantiate(numberEffect);
        NumberEffect t = new NumberEffect(g, s);
        t.RunEffect(position);
        activeNumberEffects.Enqueue(t);
    }

    void InitializeDartGame()
    {
        wrapUp.text = "";
        if(ActiveDart != null) { Destroy(ActiveDart.gameObject); }
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

    void OnGameComplete(bool forfeit)
    {
        if (ActiveDart != null) { Destroy(ActiveDart.gameObject); }
        if (!forfeit)
        {
            if (Compare(ActiveDartGame.gameScoreData, highscoreInfo) > 0)
            {
                highscoreInfo = ActiveDartGame.gameScoreData;
            }
        }
        ActiveDartGame = null;
        StartCoroutine(RunDartsReview(forfeit));
    }

    void OnForfeit(ActiveGameFunction function)
    {
        if (function == this.GameModeFunction)
        {
            OnGameComplete(true);
        }
    }

    IEnumerator RunDartsReview(bool forfeit)
    {
        while (!Input.anyKeyDown)
        {
            Debug.Log("Review darts game here");
            string s = forfeit ? "Game Forfeited!" : "Game Complete!";
            wrapUp.text = s + "\nClick any button to exit!";
            yield return null;
        }
        MinigameManager.ExitMode();
    }

    private void OnDestroy()
    {
        Dart.DartHit -= OnDartHit;
        DartGame.GameComplete -= OnGameComplete;
        MinigameManager.EnteredMode -= OnModeChange;
        HUD.ForfeitGameFunction -= OnForfeit;
        HUD.OnClickSaveData -= OnSave;
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

    public delegate void BroadcastGameComplete(bool forfeit);
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
        string darts = "Darts To Go: " +'\n' + DartsRemaining.ToString();
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
        GameComplete(false);
    }

    float ConvertPointsToCurrency()
    {
        float intoxicationMod = p.ActiveSessionIntoxication.intoxicationLevel/10f + .5f;
        float percentTotal = HelperFunctions.PercentTotal(currentPoints, StartingPointTotal)/100;

        if (percentTotal == 1f) { return currentPoints  * intoxicationMod; }
        if (percentTotal > 1f)
        {
            return currentPoints * .75f * intoxicationMod;
        }
        return (currentPoints * percentTotal) * intoxicationMod;
    }

    public float CurrentPointTotal()
    {
        return StartingPointTotal - currentPoints;
    }
}

[Serializable]
public struct NumberEffect
{
    public TextMeshPro textmesh;
    public NumberEffect(GameObject obj, int score)
    {
        textmesh = obj.GetComponent<TextMeshPro>();
        textmesh.text = "+" + score.ToString();
        textmesh.color = Color.blue;
    }

    public void RunEffect(Vector3 position)
    {
        textmesh.transform.position = position;
        textmesh.transform.localScale = Vector3.zero;
        textmesh.transform.DOScale(Vector3.one, 1);
        textmesh.DOColor(new Color(1, 0, 0, 0), 3);
    }
    public bool fadedOut { get => textmesh.color.a <= 0; }
}
