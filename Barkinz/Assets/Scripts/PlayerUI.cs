using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public TextMeshPro nameDisplay, statusDisplay;
    public SpriteRenderer statusIcon;
    ActivePlayer player;

    private void Start()
    {
        nameDisplay = transform.Find("Name Display").GetComponent<TextMeshPro>();
        statusDisplay = transform.Find("Status Display").GetComponent<TextMeshPro>();
        statusIcon = GetComponentInChildren<SpriteRenderer>();
        player = GetComponentInParent<ActivePlayer>();
    }

    private void Update()
    {
        nameDisplay.text = BarkinzManager.PrimaryBarkinz.BarkinzType;
        statusIcon.sprite = player.statusSprites[player.ActiveSessionIntoxication.IntoxicationRange];
        statusDisplay.text = player.ActiveSessionIntoxication.intoxicationMessage;
    }

    public void ToggleVisibility()
    {
        bool t = !gameObject.activeSelf;
        gameObject.SetActive(t);
    }

    public void ToggleVisibility(bool set)
    {
        gameObject.SetActive(set);
    }
}
