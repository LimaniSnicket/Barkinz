using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UserDataStorage))]
public class UserDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
       if(UserDataStorage.lookup != null && UserDataStorage.lookup.Count > 0)
        {
            EditorGUILayout.LabelField("User Lookup:");
            foreach(var k in UserDataStorage.lookup)
            {
                EditorGUILayout.LabelField(string.Format("{0}: {1} owned Barkinz", k.Key, k.Value.barkinzRedeemed));
            }
        }
    }
}
