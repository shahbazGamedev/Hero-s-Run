using UnityEngine;
using System.Collections;

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
		Invoke("Step1", 1.5f );
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

}
