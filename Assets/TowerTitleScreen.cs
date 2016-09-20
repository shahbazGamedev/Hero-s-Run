using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TowerTitleScreen : MonoBehaviour {

	public GameObject fairy;
	FairyController fairyController;
	public Lightning lightning;
	public TitleScreenHandler titleScreenHandler;

	void Step1 ()
	{
		if(PlayerStatsManager.Instance.isFirstTimePlaying() )
		{
			fairyController = fairy.GetComponent<FairyController>();
			fairyController.cutsceneAppear(FairyEmotion.Happy);
			Invoke("Step2", 1f );
		}
	}

	void Step2 ()
	{
		LeanTween.rotateX( fairy, 336f, 1.5f ).setEase(LeanTweenType.easeInOutSine);
		LeanTween.rotateY( fairy, 178f, 1.5f ).setEase(LeanTweenType.easeInOutSine);
	}

	public void playButtonPressed ()
	{
		if(PlayerStatsManager.Instance.isFirstTimePlaying() )
		{
			fairyController.cutsceneDisappear();
			Invoke("Step3", 1.2f );
		}
		else
		{
			titleScreenHandler.play();
		}
	}

	void Step3 ()
	{
		titleScreenHandler.play();
	}

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
		lightning.controlLightning(GameEvent.Start_Lightning);
		Invoke("Step1", 1.5f );
     }
}