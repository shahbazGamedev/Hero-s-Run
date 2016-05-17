using UnityEngine;
using System.Collections;

public class TreasureKeyHandler : MonoBehaviour {

	public bool playKeyTutorial = false;

	PlayerController playerController;
	FairyController fairyController;

	
	void Start ()
	{
		considerActivatingTreasureKey ();
	}

	private void considerActivatingTreasureKey ()
	{
		GameObject treasureKeyPrefab = Resources.Load( "Level/Props/Treasure Key/Treasure Key") as GameObject;
		GameObject go = (GameObject)Instantiate(treasureKeyPrefab, gameObject.transform.position, gameObject.transform.rotation );
		go.gameObject.transform.parent = gameObject.transform;
	}
	
	public void keyPickedUp()
	{
		Debug.Log("TreasureKeyHandler: Player picked up treasure key.");

		if ( playKeyTutorial )
		{
			GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
			playerController = playerObject.GetComponent<PlayerController>();
	
			GameObject fairyObject = GameObject.FindGameObjectWithTag("Fairy");
			fairyController = fairyObject.GetComponent<FairyController>();

			displayFairyMessage();
		}
		else
		{
			GetComponent<AudioSource>().Play(); //Only play pickup sound when no fairy, because fairy has an "appear" sound
		}
	}

	void displayFairyMessage()
	{
		//Call fairy
		fairyController.Appear ( FairyEmotion.Happy );

		playerController.allowRunSpeedToIncrease = false;
		Invoke ("step1", 1f );
	}

	//Fairy tells something to player
	void step1()
	{
		AchievementDisplay.activateDisplayFairy( LocalizationManager.Instance.getText("TUTORIAL_TREASURE_KEY_FAIRY"), 0.35f, 3f );
		//Player looks at fairy
		playerController.lookOverShoulder( 0.4f, 2.75f );
		Invoke ("step2", 3.5f );
	}
	
	//Make the fairy disappear
	void step2()
	{
		fairyController.Disappear ();
		playerController.allowRunSpeedToIncrease = true;
	}
}
