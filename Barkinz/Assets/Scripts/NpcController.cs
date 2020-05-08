using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, IZoomOn
{
    public Tile TileAt;
    public SpriteRenderer OverworldSpriteDisplay;
    public Transform ZoomObjectTransform { get => transform; }
    public float CameraOrthoSize => 2;
    public BarkinzInfo barkinz { get; private set; }
    public IntoxicationSettings npcIntoxication;

    public Drink isDrinking;

    public Vector3 ZoomCamPosition()
    {
        Vector3 p = transform.position + new Vector3(0, 1, -1.5f);
        return p;
    }

    private void Update()
    {
        if (isDrinking == null || isDrinking.FinishedDrink)
        {
            isDrinking = Bartender.GetDrink(barkinz);
        } else
        {
            DrinkYouLoveableDegenerateBastard();
        }
    }

    public void SetBarkinz(BarkinzInfo b, Tile t)
    {
        OverworldSpriteDisplay.sprite = b.OverworldSprite;
        transform.position = t.centerPosition;
        TileAt = t;
        barkinz = b;
        npcIntoxication = new IntoxicationSettings();
        GetComponentInChildren<NpcUI>().Initialize();
    }

    float timer = 0;
    int chugAtSecond;
    void DrinkYouLoveableDegenerateBastard()
    {
        int chug = Mathf.FloorToInt(timer);
        timer += Time.deltaTime;
        if(chug == chugAtSecond)
        {
            npcIntoxication.Intoxicate((timer - chug) * .5f, isDrinking);
        }
        if(chug > chugAtSecond)
        {
            timer = 0;
            chugAtSecond = Random.Range(5, 15);
        }
    }
}
