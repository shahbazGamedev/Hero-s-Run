using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MPGameEndManager))]
public class XPTestHelper : Editor {

	const int XP_TO_GRANT = 1000;
	public override void OnInspectorGUI()
    {
        MPGameEndManager myScript = (MPGameEndManager)target;
		EditorGUILayout.Space ();
        if(GUILayout.Button("Grant XP"))
        {
            myScript.grantTestXP(XP_TO_GRANT);
        }
		EditorGUILayout.Space ();
        DrawDefaultInspector();
     }

}
