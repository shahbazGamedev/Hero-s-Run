using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour {

	public AnimationClip doorOpen;	

	void playAnim( AnimationClip clip )
	{
		if( GetComponent<Animation>() != null )
		{
			GetComponent<Animation>()[clip.name].wrapMode = WrapMode.Once;
			GetComponent<Animation>()[clip.name].speed = 1f;
			GetComponent<Animation>().Play(clip.name);
		}
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}
	

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		print ("Player trigger event received " + eventType );
		if( eventType == GameEvent.Open_Door )
		{
			GetComponent<AudioSource>().Play();
			if( GetComponent<Animation>() != null )
			{
				playAnim(doorOpen);
			}
			else
			{
				GetComponent<Animator>().SetTrigger("open");
			}
		}
	}

	void GameStateChange( GameState previousState, GameState newState )
	{
		if( newState == GameState.Paused )
		{
			if( GetComponent<Animation>() != null )
			{
				GetComponent<Animation>().enabled = false;
			}
			else
			{
				GetComponent<Animator>().enabled = false;
			}

		}
		else if( newState == GameState.Normal )
		{
			if( GetComponent<Animation>() != null )
			{
				GetComponent<Animation>().enabled = true;
			}
			else
			{
				GetComponent<Animator>().enabled = true;
			}
		}
	}
}
