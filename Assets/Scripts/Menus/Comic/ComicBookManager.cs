using UnityEngine;
using System.Collections;

public class ComicBookManager : MonoBehaviour {

	// Use this for initialization
	void Awake () {
	
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		Invoke ("enableAutoRotation", 0.5f );
	}

	void enableAutoRotation()
	{
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
		Screen.autorotateToLandscapeLeft = true;
		Screen.autorotateToLandscapeRight = true;
		
		Screen.orientation = ScreenOrientation.AutoRotation;
	}
	
}
