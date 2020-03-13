using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class TriviaScript : MonoBehaviour
{
    public TMP_InputField AnswerInput;
    public TextMeshProUGUI QuestionText, AnswerText;
    private string hiddenAnswer, displayAnswer;
    public List<string> characterInput;
    public ActivePlayer player;

    string path;
    public TriviaLoad triviaLookup;
    public List<TriviaQuestion> availableTriviaQuestions;
    public TriviaQuestion activeTriviaQuestion;
    public Queue<TriviaQuestion> incorrectBacklog;

    Dictionary<KeyCode, char[]> KeyOffsetMapping;
    KeyCode pressed;

    string keycodePath;
    public KeyOffsetData keyOffsetData;
    public Dictionary<KeyCode, List<KeyCodeBinding>> keyOffsetDictionary;
    public OffsetType activeOffset;

    private void Start()
    {
        AnswerInput.onValueChanged.AddListener( delegate { OnAnswerInput(); });
        characterInput = new List<string>();
        displayAnswer = "";
        path = Application.streamingAssetsPath + "/TriviaData.json";
        keycodePath = Application.streamingAssetsPath + "/KeyOffsetData.json";
        LoadTriviaData();
        LoadKeyOffsetData();
        availableTriviaQuestions = new List<TriviaQuestion>(triviaLookup.TriviaQuestions);
        activeTriviaQuestion = GetTriviaQuestion();
        incorrectBacklog = new Queue<TriviaQuestion>();
        KeyOffsetMapping = GetKeyOffsetMapping();
        keyOffsetDictionary = keyOffsetData.KeyCodeToOffsetDictionary();
        activeOffset = OffsetType.None;
    }

    private void Update()
    {
        AnswerText.text = displayAnswer;
        QuestionText.text = activeTriviaQuestion.question;
        if (Input.GetKeyDown(KeyCode.Return))
        {
            AnswerQuestion();
            activeOffset = RandomizeCharacter()? (OffsetType)Mathf.FloorToInt(UnityEngine.Random.Range(1, 5)): OffsetType.None;
        }
    }

    void OnAnswerInput()
    {
        if(displayAnswer.Length <= AnswerInput.text.Length)
        {
            string toAdd = "";
            if (AnswerInput.text.Length > 0)
            {
                if (activeOffset != OffsetType.None)
                {
                    foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (Input.GetKeyDown(k) && keyOffsetDictionary.ContainsKey(k))
                        {
                            toAdd = keyOffsetDictionary[k][(int)activeOffset].keycode.ToString();
                        }
                        else if (Input.GetKeyDown(k) && !keyOffsetDictionary.ContainsKey(k))
                        {
                            toAdd = AnswerInput.text.Substring(AnswerInput.text.Length - 1);
                        }
                    }
                } else
                {
                    toAdd = AnswerInput.text.Substring(AnswerInput.text.Length - 1);
                }
                characterInput.Add(toAdd);
            }
            if (!typing) { StartCoroutine(MaintainDisplayWhileIntoxicated()); }
        } else
        {
            if (characterInput.Count > 0)
            {
                characterInput.RemoveAt(characterInput.Count - 1);
            }
            displayAnswer = displayAnswer.Remove(displayAnswer.Length - 1);
            Debug.Log("remove chars at end of display");
        }
    }

    void AnswerQuestion()
    {
        if (activeTriviaQuestion.Evaluate(displayAnswer))
        {
            float reward = Mathf.Max(1, player.ActiveSessionIntoxication.intoxicationLevel / 100 * activeTriviaQuestion.pointWorth);
            MinigameManager.RewardPlayer(reward);
        } else
        {
            incorrectBacklog.Enqueue(activeTriviaQuestion);
        }
        displayAnswer = "";
        AnswerInput.text = "";
        characterInput.Clear();
        activeTriviaQuestion = GetTriviaQuestion();
        if(incorrectBacklog != null && incorrectBacklog.Count > 0 && incorrectBacklog.Count % 3 == 0) { availableTriviaQuestions.Add(incorrectBacklog.Dequeue()); }
        
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

    void LoadTriviaData()
    {
        string json = File.ReadAllText(path);
        triviaLookup = JsonUtility.FromJson<TriviaLoad>(json);
    }

    void LoadKeyOffsetData()
    {
        string json = File.ReadAllText(keycodePath);
        keyOffsetData = JsonUtility.FromJson<KeyOffsetData>(json);
    }

    TriviaQuestion GetTriviaQuestion()
    {
        int rand = (int)UnityEngine.Random.Range(0, availableTriviaQuestions.Count);
        TriviaQuestion tq = availableTriviaQuestions[rand];
        availableTriviaQuestions.RemoveAt(rand);
        return tq;
    }

    Dictionary<KeyCode, char[]> GetKeyOffsetMapping()
    {
        Dictionary<KeyCode, char[]> r = new Dictionary<KeyCode, char[]>();
        r.Add(KeyCode.A, new char[] {'q', 's', 'z'});
        r.Add(KeyCode.I, new char[] { 'u', 'k', 'o'});
        r.Add(KeyCode.U, new char[] { 'y', 'i', 'j' });
        r.Add(KeyCode.O, new char[] { 'i', 'p', 'l'});
        r.Add(KeyCode.E, new char[] { 'w', 'd', 'r'});
        return r;
    }

    bool RandomizeCharacter()
    {
        float rand = UnityEngine.Random.Range(0, 100);
        return player.ActiveSessionIntoxication.intoxicationLevel >= rand;
    }

    char GetChar(KeyCode k)
    {
        char[] possibilities = KeyOffsetMapping[k];
        return possibilities[(int)UnityEngine.Random.Range(0, possibilities.Length)];
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

[Serializable]
public class KeyOffsetData
{
    public List<KeyCodeOffset> KeyOffsetMaps;
    public Dictionary<KeyCode, List<KeyCodeBinding>> KeyCodeToOffsetDictionary()
    {
        Dictionary<KeyCode, List<KeyCodeBinding>> keyValuePairs = new Dictionary<KeyCode, List<KeyCodeBinding>>();
        foreach (KeyCodeOffset k in KeyOffsetMaps)
        {
            KeyCode key;
            Enum.TryParse<KeyCode>(k.PrimaryKey, out key);
            keyValuePairs.Add(key, CreateKeyCodeBindingList(k.OffsetPairs));
        }
        return keyValuePairs;
    }

    List<KeyCodeBinding> CreateKeyCodeBindingList(List<KeyBinding> k)
    {
        List<KeyCodeBinding> kb = new List<KeyCodeBinding>();
        foreach (KeyBinding bind in k)
        {
            kb.Add(new KeyCodeBinding(bind));
        }
        return kb;
    }
}

[Serializable]
public class KeyCodeOffset
{
    public string PrimaryKey;
    public List<KeyBinding> OffsetPairs;
}

[Serializable]
public class KeyBinding
{
    public string Key;
    public OffsetType Offset;
}

[Serializable]
public class KeyCodeBinding
{
    public KeyCode keycode;
    public OffsetType offset;
    public KeyCodeBinding(KeyBinding k)
    {
        Enum.TryParse<KeyCode>(k.Key, out keycode);
        offset = k.Offset;
    }
}

public enum OffsetType
{
    None = 0,
    LeftOne = 1,
    UpOne = 2,
    RightOne = 3,
    DownOne = 4,
    LeftTwo = 5,
    UpTwo = 6,
    RightTwo = 7,
    DownTwo = 8
}

