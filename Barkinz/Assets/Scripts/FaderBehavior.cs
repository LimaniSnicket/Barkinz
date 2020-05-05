using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FaderBehavior : MonoBehaviour
{
    private static FaderBehavior fader;
    private static Image faderImage;

    public static bool transparent { get => Mathf.Approximately(faderImage.color.a, 0); }
    public static bool fullAlpha { get => Mathf.Approximately(faderImage.color.a, 1); }

    private void Awake()
    {
        if (fader == null) { fader = this; } else { Destroy(gameObject); }
        faderImage = GetComponentInChildren<Image>();
        DontDestroyOnLoad(gameObject);
    }

     public static void DoFade(float time = 3)
    {
        float end = transparent ? 1 : 0;
        faderImage.DOFade(end, time);
    }

    public static void DoFadeColor(Color[] colors, float delay, float time = 0.5f)
    {
        fader.StartCoroutine(FadeColors(colors, time, delay));
    }

    public static void DoFadeColor(float time = 3)
    {
        DoFadeColor(Color.white, time);
    }

    public static void DoFadeColor(Color c, float time = 3)
    {
        faderImage.DOColor(c, time);
    }

    static IEnumerator FadeColors(Color[] c, float t, float d)
    {
        if(c.Length <= 0) { yield return null; }
        yield return new WaitForSeconds(d);
        int tr =0;
        while (tr < c.Length)
        {
            faderImage.DOColor(c[tr], t);
            yield return new WaitForSeconds(t);
            tr++;
        }
    }
}
