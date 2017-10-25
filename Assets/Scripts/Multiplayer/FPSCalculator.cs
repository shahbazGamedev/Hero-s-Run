using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCalculator : MonoBehaviour {

	string fps = "0";
	float fpsAccumulator = 0;
	//we do not want to update the fps value every frame
	int fpsFrameCounter = 0;
	int fpsWaitFrames = 30;

	public string getFPS ()
	{
		return fps;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//average out the fps over a number of frames
		fpsFrameCounter++;
		fpsAccumulator = fpsAccumulator + Time.deltaTime;
		if ( fpsFrameCounter == fpsWaitFrames )
		{
			fpsFrameCounter = 0;
			fps = Mathf.Ceil(fpsWaitFrames/fpsAccumulator).ToString();
			fpsAccumulator = 0;
		}	
	}

}
