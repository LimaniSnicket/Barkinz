using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraEffectManager : MonoBehaviour
{
    private static CameraEffectManager camEffects;
    public static ChromaticAberration chromaticAberration;

    private void Awake()
    {
        if (camEffects==null) { camEffects = this; } else { Destroy(this); }
    }
}
