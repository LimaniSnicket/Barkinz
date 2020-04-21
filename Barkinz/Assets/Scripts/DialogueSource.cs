using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSource : MonoBehaviour, IZoomOn
{
    public string dialogueFileName;
    public string filePath { get => Application.streamingAssetsPath +"/"+ dialogueFileName + ".json"; }

    public Transform ZoomObjectTransform => transform;
    public float CameraOrthoSize => 3;

    public int startIndex;

    private void Awake()
    {
        
    }

    void UpdateStartIndexOnComplete(int newStartIndex)
    {

    }

    private void OnDestroy()
    {
        
    }

    public Vector3 ZoomCamPosition()
    {
        Vector3 p = transform.position + new Vector3(1, 1, -1);
        return p;
    }
}
