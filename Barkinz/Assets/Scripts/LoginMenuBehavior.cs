using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class LoginMenuBehavior : MonoBehaviour
{
    public TextMeshProUGUI displayMessage;
    public TMP_InputField userInput;
    public Button generateKey, logIn;
    string user { get => userInput.text ?? ""; }
    string defaultMessage = "Enter User Key or Create a new Key!";

    private void Start()
    {
        if(generateKey != null) { generateKey.onClick.AddListener(()=> OnClickGetNewKey()); }
        if (logIn != null) { logIn.onClick.AddListener(() => OnClickProceedWithUser()); }
    }

    private void Update()
    {
        displayMessage.GetComponent<RectTransform>().PulseText(1f, 0.1f, 0.5f);
    }

    public void OnClickProceedWithUser()
    {
        //Vector3 current = userInput.GetComponent<RectTransform>().position;
        //userInput.GetComponent<RectTransform>().DOPath(new Vector3[] { current + new Vector3(0, 25, 0), current + new Vector3(0,50,0),current + new Vector3(0,15,0), current + new Vector3(0, -200, 0) }, 2, PathType.Linear);
        if (!UserDataStorage.ValidateUser(user) && !UserDataStorage.ValidateUser(user, true))
        {
            Debug.Log("User doesn't exist. Try again?");
            StartCoroutine(ErrorMessageDisplay("User doesn't exist. Try again?"));
        } 
        else
        {
            //userInput.GetComponent<RectTransform>().DOJump(new Vector3(-100, -100), 10, 1, 10);
            if (UserDataStorage.NewUser(user)) { UserDataStorage.AddUser(user); }
            UserDataStorage.SetActiveUserKey(user);
            UserDataStorage.LoadUserData(user);
            SceneManager.LoadScene(1);
        }
    }

    public void OnClickGetNewKey()
    {
        Debug.Log("Getting a new key for you!");
        StringBuilder builder = new StringBuilder();
        System.Random rand = new System.Random();
        char c;
        for (int i =0; i < 8; i++)
        {
            int charIndex = (int)Math.Floor(rand.NextDouble() * 25);
            c = Convert.ToChar(charIndex + 65);
            builder.Append(c);
        }
        if (UserDataStorage.ValidateUser(builder.ToString()))
        {
            OnClickGetNewKey();
        } else
        {
            userInput.text = builder.ToString();
            UserDataStorage.SetTempUserKey(builder.ToString());
        }
    }

    IEnumerator ErrorMessageDisplay(string errorMessage)
    {
        displayMessage.text = string.Format("<color=red>{0}</color>", errorMessage);
        yield return new WaitForSeconds(2);
        displayMessage.text = string.Format("<color=black>{0}</color>", defaultMessage);
    }


}

