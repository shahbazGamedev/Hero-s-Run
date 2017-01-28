using UnityEngine;
using System.Collections;

public class TowerTitleScreen : MonoBehaviour {

	public GameObject fairy;
	FairyController fairyController;
	public Lightning lightning;
	public TitleScreenHandler titleScreenHandler;

	void Start()
	{
		lightning.controlLightning(GameEvent.Start_Lightning);
		Invoke("Step1", 3f );
	}

	void Step1 ()
	{
		if(PlayerStatsManager.Instance.isFirstTimePlaying() )
		{
			fairyController = fairy.GetComponent<FairyController>();
			fairyController.cutsceneAppear(FairyEmotion.Happy);
			Invoke("Step2", 2f );
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
}