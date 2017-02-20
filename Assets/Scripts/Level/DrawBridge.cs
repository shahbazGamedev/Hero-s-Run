using UnityEngine;
using System.Collections;

public class DrawBridge : MonoBehaviour {
	
	//The X rotation the Drawbridge will have when it is fully raised
	public float bridgeRaisedAngle = -30f;
	//How long will it take for the bridge to raise
	public float duration = 4f; 

	void raiseDrawBridge()
	{
		LeanTween.rotateX( gameObject, bridgeRaisedAngle, duration ).setEase(LeanTweenType.easeInOutQuad);
		GetComponent<AudioSource>().Play();
	}
	
	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		PlayerTrigger.playerEnteredTrigger += PlayerEnteredTrigger;
		//Reset the bridge X rotation to 0. This is important because the tile can be recycled.
		transform.rotation = Quaternion.Euler( 0, transform.eulerAngles.y, transform.eulerAngles.z );
		LeanTween.cancel( gameObject );
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		PlayerTrigger.playerEnteredTrigger -= PlayerEnteredTrigger;
	}

	void PlayerEnteredTrigger( GameEvent eventType, GameObject uniqueGameObjectIdentifier )
	{
		if( eventType == GameEvent.Raise_Draw_Bridge )
		{
			//Is this event for me specifically?
			if( gameObject == uniqueGameObjectIdentifier )
			{
				raiseDrawBridge();
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
