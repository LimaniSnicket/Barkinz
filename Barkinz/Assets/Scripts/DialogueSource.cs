using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSource : MonoBehaviour
{
    public string dialogueFileName;
    public string filePath { get => Application.streamingAssetsPath +"/"+ dialogueFileName + ".json"; }
}
