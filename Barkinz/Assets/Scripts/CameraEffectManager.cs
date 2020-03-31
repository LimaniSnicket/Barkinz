using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraEffectManager : MonoBehaviour
{
    private static CameraEffectManager camEffects;
    public static ChromaticAberration chromaticAberration;
    public static Vignette vignette;
    public float defaultVignette;
    ActivePlayer activePlayer;

    private void Awake()
    {
        if (camEffects==null) { camEffects = this; } else { Destroy(this); }
        ActivePlayer.SetActivePlayer += SetActivePlayer;
    }

    private void Start()
    {
        GetComponent<PostProcessVolume>().profile.TryGetSettings(out chromaticAberration);
        GetComponent<PostProcessVolume>().profile.TryGetSettings(out vignette);
    }

    private void Update()
    {
        if (activePlayer != null)
        {
            chromaticAberration.intensity.value = (activePlayer.ActiveSessionIntoxication.intoxicationLevel / 100) * 1.5f + Mathf.Sin(Time.time * Mathf.PI) * 0.01f;
            vignette.intensity.value = defaultVignette + (Mathf.Sin(Time.time * Mathf.PI) /27);
        }
    }

    void SetActivePlayer(ActivePlayer p)
    {
        activePlayer = p;
    }

    private void OnDestroy()
    {
        ActivePlayer.SetActivePlayer -= SetActivePlayer;
    }
}
