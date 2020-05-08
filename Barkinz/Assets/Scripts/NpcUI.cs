using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NpcUI : PlayerUI
{
    NpcController npc;
    public Sprite[] statusIcons;
    public SpriteRenderer DrinkImage;

    public void Initialize()
    {
        GetTMPsAndShit();
        npc = GetComponentInParent<NpcController>();
        DrinkImage = transform.Find("DrinkDisplay").GetComponent<SpriteRenderer>();
        DrinkImage.sprite = Resources.Load<Sprite>(npc.isDrinking.displaySpritePath);
    }

    private void Update()
    {
        nameDisplay.text = npc.barkinz.BarkinzType;
        statusDisplay.text = npc.npcIntoxication.intoxicationMessage;
        statusIcon.sprite = statusIcons[npc.npcIntoxication.IntoxicationRange];
    }
}
