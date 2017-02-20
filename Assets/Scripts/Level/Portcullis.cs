using UnityEngine;
using System.Collections;

public class Portcullis : MonoBehaviour {
	
	//The distance in Y that the portcullis will lower
	public float yDistance;
	//How long will it take for the gate to reach its destination
	public float duration = 1f; 

	float portcullisYpos = 0;

	void Awake()
	{
		//Remember the portcullis' Y local position so we can reset it later
		portcullisYpos = transform.localPosition.y;
	}

	void lowerGate()
	{
		float endPositionY = transform.localPosition.y - yDistance;
		LeanTween.moveLocalY( gameObject, endPositionY, duration ).setEase(LeanTweenType.easeOutExpo);
		GetComponent<AudioSource>().Play();
	}
	
	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
		//Reset portcullis' Y position because the tile may be recycled
		transform.localPosition = new Vector3( transform.localPosition.x, portcullisYpos, transform.localPosition.z );
		LeanTween.cancel( gameObject );
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Lower_Gate )
		{
			//Is this event for me specifically?
			if( gameObject == uniqueGameObjectIdentifier )
			{
				lowerGate();
			}
		}
	}
	
	void GameStateChange( GameState previousState, GameState newState )
	{
		if( newState == GameState.Paused )
		{
			LeanTween.pause( gameObject );
		}
		else if( newState == GameState.Normal )
		{
			LeanTween.resume( gameObject );
		}
	}
}
