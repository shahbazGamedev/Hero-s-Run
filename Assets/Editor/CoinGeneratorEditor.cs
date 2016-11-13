using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor(typeof(SegmentInfo))]
public class CoinGeneratorEditor : Editor {

	public override void OnInspectorGUI()
    {
        SegmentInfo myScript = (SegmentInfo)target;
		EditorGUILayout.Space ();
        if(GUILayout.Button("Generate Coins"))
        {
            myScript.addCoins();
        }
		EditorGUILayout.Space ();
        DrawDefaultInspector();
     }

}
