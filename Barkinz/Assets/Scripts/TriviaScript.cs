using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class TriviaScript : MonoBehaviour
{
    public GameObject questionUI;
    public TMP_InputField AnswerInput;
    public TextMeshProUGUI QuestionText, AnswerText;
    private string hiddenAnswer, displayAnswer;
    public List<char> characterInput;
    public ActivePlayer player;
    string dialoguePath;

    string path;
    public TriviaLoad triviaLookup;
    public List<TriviaQuestion> availableTriviaQuestions;
    public TriviaQuestion activeTriviaQuestion;
    public Queue<TriviaQuestion> incorrectBacklog;

    public GameObject BettingUIParent;
    public BettingData betData;

    bool bettingPhase;

    public int OffsetAmount;

    private void Start()
    {
        AnswerInput.onValueChanged.AddListener( delegate { OnAnswerInputDoCaesarCipher(); });
        characterInput = new List<char>();
        displayAnswer = "";
        path = Application.streamingAssetsPath + "/TriviaData.json";
        LoadTriviaData();
        availableTriviaQuestions = new List<TriviaQuestion>(triviaLookup.TriviaQuestions);
        activeTriviaQuestion = GetTriviaQuestion();
        incorrectBacklog = new Queue<TriviaQuestion>();
        betData = new BettingData(BettingUIParent);
        betData.readyButton.onClick.AddListener(()=> OnClickPlaceBet());
        OffsetAmount = 0;
    }

    string answerStatus;

    private void Update()
    {
        AnswerText.text = displayAnswer;
        QuestionText.text = bettingPhase? answerStatus + " Alright! Next Question, comin' up whenever you're ready!" : activeTriviaQuestion.question;
        if (Input.GetKeyDown(KeyCode.Return) && !bettingPhase)
        {
            AnswerQuestion();
            OffsetAmount = Mathf.FloorToInt(player.ActiveSessionIntoxication.intoxicationLevel/10);
            bettingPhase = true;
        }
        betData.gameObject.SetActive(bettingPhase);
        betData.activeBetDisplay.text = betData.betDisplayMessage;
    }

    void OnAnswerInputDoCaesarCipher()
    {
        if (AnswerInput.text.Length > 0)
        {
            char input = CaesarCipher(AnswerInput.text[AnswerInput.text.Length - 1], OffsetAmount);
            Debug.Log(input);
            characterInput.Add(input);
            StartCoroutine(MaintainDisplayWhileIntoxicated());
        }
    }

    void AnswerQuestion()
    {
        if (activeTriviaQuestion.Evaluate(displayAnswer))
        {
            float reward = Mathf.Max(1, player.ActiveSessionIntoxication.intoxicationLevel / 100 * activeTriviaQuestion.pointWorth * betData.activeBetAmount);
            MinigameManager.RewardPlayer(reward);
            answerStatus = "Correct!";
        } else
        {
            incorrectBacklog.Enqueue(activeTriviaQuestion);
            answerStatus = "Oops! Incorrect! <size=45>haha idiot</size>";
        }
        displayAnswer = "";
        AnswerInput.text = "";
        characterInput.Clear();
        if (incorrectBacklog != null && incorrectBacklog.Count > 0 && incorrectBacklog.Count % 3 == 0) { availableTriviaQuestions.Add(incorrectBacklog.Dequeue()); }
        
    }

    public bool typing = false;
    public IEnumerator MaintainDisplayWhileIntoxicated()
    {
        typing = true;
        while(characterInput.Count >= 1)
        {
            displayAnswer += characterInput[0];
            characterInput.RemoveAt(0);
            yield return null;
        }
        typing = false;
    }

    void OnClickPlaceBet()
    {
        MinigameManager.activeCurrency -= betData.activeBetAmount;
        bettingPhase = false;
        activeTriviaQuestion = GetTriviaQuestion();
    }

    void LoadTriviaData()
    {
        string json = File.ReadAllText(path);
        triviaLookup = JsonUtility.FromJson<TriviaLoad>(json);
    }

    TriviaQuestion GetTriviaQuestion()
    {
        int rand = (int)UnityEngine.Random.Range(0, availableTriviaQuestions.Count);
        TriviaQuestion tq = availableTriviaQuestions[rand];
        availableTriviaQuestions.RemoveAt(rand);
        return tq;
    }


    bool RandomizeCharacter()
    {
        float rand = UnityEngine.Random.Range(0, 100);
        return player.ActiveSessionIntoxication.intoxicationLevel >= rand;
    }

    public char CaesarCipher(char input, int shift)
    {
        if(shift == 0) { return input; }
        return (char)((int)input + shift);
    }

}

[Serializable]
public class BettingData
{
    public GameObject gameObject { get; private set; }
    public int activeBetAmount { get; private set; }
    public Button subtractButton, addButton, readyButton;
    public TextMeshProUGUI activeBetDisplay;
    int minBet = 1;
    int maxBet = 5;
    public BettingData(GameObject parent)
    {
        gameObject = parent;
        activeBetAmount = minBet;
        subtractButton = parent.transform.Find("Subtract Button").GetComponent<Button>();
        addButton = parent.transform.Find("Add Button").GetComponent<Button>();
        subtractButton.onClick.AddListener(()=> AdjustBettingAmount(-1));
        addButton.onClick.AddListener(()=> AdjustBettingAmount(1));
        readyButton = parent.transform.Find("Ready Button").GetComponent<Button>();
        activeBetDisplay = parent.transform.Find("Bet Display").GetComponent<TextMeshProUGUI>();
    }

    public string betDisplayMessage { get => "$: " + activeBetAmount.ToString(); }

    public void AdjustBettingAmount(int adjustment)
    {
        if(adjustment < 0) { if (activeBetAmount>minBet) { activeBetAmount--; } }
        if (adjustment > 0) { if (activeBetAmount < maxBet) { activeBetAmount++; } }
    }

}

[Serializable]
public class TriviaLoad
{
    public List<TriviaQuestion> TriviaQuestions;
}

[Serializable]
public class TriviaQuestion
{
    public string question, answer;
    public int pointWorth;
    public TriviaQuestion() { }
    public TriviaQuestion(string q, string a)
    {
        question = q;
        answer = a;
        pointWorth = 10;
    }
    public TriviaQuestion(string q, string a, int p)
    {
        question = q;
        answer = a;
        pointWorth = p;
    }
    public bool Evaluate(string a)
    {
        return string.Equals(a, answer, StringComparison.OrdinalIgnoreCase);
    }
}







