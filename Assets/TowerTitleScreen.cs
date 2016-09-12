using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TowerTitleScreen : MonoBehaviour {

	public GameObject fairy;
	FairyController fairyController;
	public Lightning lightning;
	public TitleScreenHandler titleScreenHandler;

	// Use this for initialization
	void Start ()
	{
		fairyController = fairy.GetComponent<FairyController>();
		lightning.controlLightning(GameEvent.Start_Lightning);
		Invoke("Step1", 3f );
	}

	void Step1 ()
	{
		fairyController.cutsceneAppear(FairyEmotion.Happy);
		Invoke("Step2", 1f );
	}

	void Step2 ()
	{
		LeanTween.rotateX( fairy, 336f, 1.5f ).setEase(LeanTweenType.easeInOutSine);
		LeanTween.rotateY( fairy, 178f, 1.5f ).setEase(LeanTweenType.easeInOutSine);
	}

	public void playButtonPressed ()
	{
		fairyController.cutsceneDisappear();
		Invoke("Step3", 1.2f );
	}

	void Step3 ()
	{
		titleScreenHandler.play();
	}

	/* Seems to require 5.4
	void OnEnable()
     {
      //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
         SceneManager.sceneLoaded += OnLevelFinishedLoading;
     }
 
     void OnDisable()
     {
     //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
         SceneManager.sceneLoaded -= OnLevelFinishedLoading;
     }
 
     void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
     {
         Debug.Log("Level Loaded");
         Debug.Log(scene.name);
         Debug.Log(mode);
     }*/
}