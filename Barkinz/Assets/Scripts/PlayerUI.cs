using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public TextMeshPro nameDisplay, statusDisplay;
    public SpriteRenderer statusIcon;
    ActivePlayer player;

    private void Awake()
    {
        CameraMovement.ZoomedOnObject += OnZoomed;
    }

    private void Start()
    {
        nameDisplay = transform.Find("Name Display").GetComponent<TextMeshPro>();
        statusDisplay = transform.Find("Status Display").GetComponent<TextMeshPro>();
        statusIcon = GetComponentInChildren<SpriteRenderer>();
        player = GetComponentInParent<ActivePlayer>();
        ToggleVisibility(false);
    }

    private void Update()
    {
        nameDisplay.text = BarkinzManager.PrimaryBarkinz.BarkinzType;
        statusIcon.sprite = player.statusSprites[player.ActiveSessionIntoxication.IntoxicationRange];
        statusDisplay.text = player.ActiveSessionIntoxication.intoxicationMessage;
    }

    void OnZoomed(IZoomOn zoom)
    {
        Debug.Log(zoom.GetType().ToString());
        if (zoom.GetType().ToString() == "ActivePlayer")
        {
            ToggleVisibility(true);
        }
    }

    public void ToggleVisibility(bool set)
    {
        nameDisplay.gameObject.SetActive(set);
        statusDisplay.gameObject.SetActive(set);
    }

    private void OnDestroy()
    {
        CameraMovement.ZoomedOnObject -= OnZoomed;
    }
}
