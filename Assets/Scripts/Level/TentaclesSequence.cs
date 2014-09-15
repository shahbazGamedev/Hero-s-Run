using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TentaclesSequence : MonoBehaviour {

	PlayerController playerController;
	FairyController fairyController;

	float lastActivateTime = 0;

	public GameObject tentaclePrefab;
	public GameObject debrisPrefab;

	float tentacleHalfHeight = 2f;
	Transform player;

	int rowIndex = 0;
	const int NUMBER_OF_ROWS = 46;
	GameEventManager gem;
	public ParticleSystem tentacleAboutToAppearFx;

	// Use this for initialization
	void Awake () {

		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		player = playerObject.transform;
		playerController = playerObject.GetComponent<PlayerController>();

		GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
		fairyController = fairyObject.GetComponent<FairyController>();
	}
	
	void Start()
	{

		GameObject gameEventManagerObject = GameObject.FindGameObjectWithTag("GameEventManager");
		gem = gameEventManagerObject.GetComponent<GameEventManager>();
		gem.setOpeningSequence( this );
	}
	
	//Fairy tells something to player
	void step1()
	{
		AchievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("FAIRY_DRAGON_BRIDGE"), 0.35f, 3.6f );
		Invoke ("step2", 3.75f );
	}

	//Fairy cast spell;
	void step2()
	{
		fairyController.CastSpell();
		Invoke ("step3", 4.2f );
	}

	//Spell works and bridge is rebuilt
	//Dragon takes-off
	void step3()
	{
		Invoke ("step4", 2.5f );
	}
	
	//Make the fairy disappear
	//Player starts running again
	void step4()
	{
		fairyController.Disappear ();
		playerController.allowRunSpeedToIncrease = true;
		playerController.startRunning(false);
		fairyController.resetYRotationOffset();
	}
	
	

}
