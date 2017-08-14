using UnityEngine;
using System.Collections;

//You can place this trigger in your scene to automatically take a picture that the player will be able to share on social media.
public class TakePictureTrigger : MonoBehaviour {

	//When triggered, this class will send a takePictureNowTrigger evemt.
	//The TakeScreenshot class will receive it, and take a picture which the player will be able to share on social media.
	public delegate void TakePictureNowTrigger( Camera pictureCamera, float flashLightIntensity );
	public static event TakePictureNowTrigger takePictureNowTrigger;
	[Tooltip("The camera to use for the picture. It should be initially disabled. It will get enabled when triggered. You can add post-processing components to the camera such as bloom and depth of field.")]
	public Camera pictureCamera;
	public bool useTrigger = true;
	[Range(0,8f)]
	public float flashLightIntensity = 1.2f;

	void OnTriggerEnter(Collider other)
	{
		if( useTrigger && other.CompareTag("Player") )
		{
			if(takePictureNowTrigger != null) takePictureNowTrigger( pictureCamera, flashLightIntensity );
		}
	}

	public void takePicture()
	{
		if(takePictureNowTrigger != null) takePictureNowTrigger( pictureCamera, flashLightIntensity );
	}
}
