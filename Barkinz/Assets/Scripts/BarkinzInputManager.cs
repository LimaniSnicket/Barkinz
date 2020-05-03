using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BarkinzInputManager : MonoBehaviour
{
    public TMP_InputField CodeInputField;
    public Button RedeemButton, StartButton;
    public Image BarkinzProfile, BG;
    public TextMeshProUGUI BarkinzNameDisplay;

    const string nullBarkinz = "NO ACTIVE BARKINZ";
    private string inputCode { get => CodeInputField.text.ToUpper(); }
    bool canPulse;

    private void Start()
    {
        RedeemButton.onClick.AddListener(() => OnClickRedeemBarkinz());
        StartButton.onClick.AddListener(() => OnClickGoToScene());
        StartButton.interactable = false;
        BarkinzProfile.gameObject.SetActive(false);
        BarkinzNameDisplay.text = nullBarkinz;
    }

    private void Update()
    {
        if (!CodeValid()) { BarkinzProfile.gameObject.SetActive(false); BarkinzNameDisplay.text = nullBarkinz; canPulse = false; } else
        {
            if (canPulse) { StartButton.GetComponent<RectTransform>().PulseText(1f, .2f, .8f); }
        }
        BG.color = HelperFunctions.Spectrum();
    }

    bool CodeValid()
    {
        return BarkinzManager.BarkinzCodeLookup.ContainsKey(inputCode);
    }

    void OnClickRedeemBarkinz()
    {
        if (CodeValid())
        {
            BarkinzInfo b = BarkinzManager.BarkinzCodeLookup[inputCode];
            try { UserDataStorage.activeUserData.RedeemBarkinz(b.name); } catch (System.NullReferenceException) { Debug.Log("User Data Storage is not present in scene"); }
            BarkinzProfile.gameObject.SetActive(true);
            BarkinzProfile.sprite = b.MainBarkinzSprite;
            BarkinzNameDisplay.text = b.BarkinzType.ToUpper();
            BarkinzManager.PrimaryBarkinz = b;
            StartButton.interactable = true;
            canPulse = true;
        } else
        {
            StartButton.interactable = false;
        }
    }

    void OnClickGoToScene()
    {
        StartCoroutine(OnClickInitializeGame());
    }

    IEnumerator OnClickInitializeGame()
    {
        try
        {
            BarkinzManager.PrimaryBarkinz.barkinzData = UserDataStorage.activeUserData.activeBarkinzData;
        } catch (System.NullReferenceException) { print("User Data Not Present in Scene!"); }
        print("Prepping to Load");
        FaderBehavior.DoFadeColor(2);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(2);
    }
}
