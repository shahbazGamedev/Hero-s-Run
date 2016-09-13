using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TreasureKeyHandler : MonoBehaviour {

	public bool playKeyTutorial = false;
	[Tooltip("You can only get a key once even if you replay an episode. Keys found only get reset if you complete the game and start over. Eack key has a unique id. The id format is THEME_TILE_NAME_X, where X is a number. You cannot find treasure keys in the endless mode, only in the story mode.")]
	public string treasureKeyID = "THEME_TILE_NAME_X";

	PlayerController playerController;
	FairyController fairyController;

	
	void Start ()
	{
		if( treasureKeyID != "THEME_TILE_NAME_X" )
		{
			considerActivatingTreasureKey();
		}
		else
		{
			Debug.LogError("TreasureKeyHandler-treasureKeyID error. The treasureKeyID is using the default value and has not been set in tile: " + transform.parent.name );
		}
	}

	private void considerActivatingTreasureKey ()
	{
		//You cannot find treasure keys in the endless mode, only in the story mode.
		if( GameManager.Instance.getGameMode() == GameMode.Story )
		{
			if( !PlayerStatsManager.Instance.hasThisTreasureKeyBeenFound( treasureKeyID ) )
			{
				GameObject treasureKeyPrefab = Resources.Load( "Level/Props/Treasure Key/Treasure Key") as GameObject;
				GameObject go = (GameObject)Instantiate(treasureKeyPrefab, gameObject.transform.position, gameObject.transform.rotation );
				go.gameObject.transform.parent = gameObject.transform;
			}
		}
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
		HUDHandler.hudHandler.displayTreasureKeyPickup();
		PlayerStatsManager.Instance.foundTreasureKey( treasureKeyID );
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
		fairyController.speak("TUTORIAL_TREASURE_KEY_FAIRY", 3f, false );
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
