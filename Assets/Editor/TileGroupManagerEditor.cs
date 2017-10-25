using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TileGroupManager))]
public class TileGroupManagerEditor : Editor
{
	//To force the save add one tile group and then delete it to make the list dirty
	public override void OnInspectorGUI()
    {
        TileGroupManager myScript = (TileGroupManager)target;
		EditorGUILayout.Space ();
        if(GUILayout.Button("Sort Tile Groups"))
        {
            myScript.sortTileGroups();
        }
		EditorGUILayout.Space ();
        DrawDefaultInspector();
     }
}
