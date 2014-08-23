using UnityEngine;
using UnityEditor;
using System.Collections;


#if UNITY_IPHONE || UNITY_ANDROID

[CustomEditor( typeof( UpsightMonoBehavior ) )]
public class UpsightMonoBehaviorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		UpsightDemoUIEditor.drawInfoBox();
		DrawDefaultInspector();
	}
}

#endif