using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour {

	Animation animation;
	Animator animator;
	public AnimationClip doorOpen;

	// Use this for initialization
	void Start ()
	{
		animation = GetComponent<Animation>();
		if( animation == null )
		{
			animator = GetComponent<Animator>();
		}
	}
	

	void playAnim( AnimationClip clip )
	{
		if( animation != null )
		{
			animation[clip.name].wrapMode = WrapMode.Once;
			animation[clip.name].speed = 1f;
			animation.Play(clip.name);
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
			audio.Play();
			if( animation != null )
			{
				playAnim(doorOpen);
			}
			else
			{
				animator.SetTrigger("open");
			}
		}
	}

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Paused )
		{
			if( animation != null )
			{
				animation.enabled = false;
			}
			else
			{
				animator.enabled = false;
			}

		}
		else if( newState == GameState.Normal )
		{
			if( animation != null )
			{
				animation.enabled = true;
			}
			else
			{
				animator.enabled = true;
			}
		}
	}
}
