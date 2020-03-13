using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(BarkinzManager))]
public class BarkinzManagerEditor : Editor
{
    string clearName;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        string d = BarkinzManager.PrimaryBarkinz != null ? BarkinzManager.PrimaryBarkinz.BarkinzType : "NONE";
        GUILayout.Label("ACTIVE: " + d, EditorStyles.boldLabel);
        for (int i =0; i < BarkinzManager.BarkinzCodeLookup.Keys.Count; i++)
        {
            GUILayout.Label(BarkinzManager.BarkinzCodeLookup.ElementAt(i).Value.BarkinzType + ": " + BarkinzManager.BarkinzCodeLookup.ElementAt(i).Key);
        }

        clearName = EditorGUILayout.TextField(clearName);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Specific Barkinz"))
        {
            BarkinzManager.BarkinzCodeLookup[clearName].ClearData();
        }
        if (GUILayout.Button("Clear all Barkinz"))
        {
            for (int j =0; j < BarkinzManager.BarkinzCodeLookup.Count; j++)
            {
                BarkinzManager.BarkinzCodeLookup.ElementAt(j).Value.ClearData();
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
