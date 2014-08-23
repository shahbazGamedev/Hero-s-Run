using UnityEngine;
using UnityEditor;
using System.Collections;


#if UNITY_IPHONE || UNITY_ANDROID

[CustomEditor( typeof( UpsightDemoUI ) )]
public class UpsightDemoUIEditor : Editor
{
	public static void drawInfoBox()
	{
		EditorGUILayout.HelpBox( "Insert your Upsight credentials from the web portal here. Note that if you are only supporting one platform you only need to enter that platform's token/secret.", MessageType.Info );
	}


	public override void OnInspectorGUI()
	{
		drawInfoBox();
		DrawDefaultInspector();
	}
}

#endif