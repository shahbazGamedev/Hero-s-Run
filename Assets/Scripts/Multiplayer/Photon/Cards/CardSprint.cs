using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
//// The Sprint card is a Common card with 13 levels.
/// </summary>
public class CardSprint : Photon.PunBehaviour {

	[SerializeField] AudioClip  soundFx;
	[SerializeField] float  baseDuration = 1.5f;
	[SerializeField] float  baseSpeed = 1.5f;
	[SerializeField] float  durationUpgradePerLevel = 0.15f;
	[SerializeField] float  speedUpgradePerLevel = 0.06f;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardSprintRPC", PhotonTargets.AllViaServer, level, photonViewId );	
	}

	[PunRPC]
	void cardSprintRPC( int level, int photonViewID )
	{
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				startSprint( level, PlayerRace.players[i].GetComponent<PlayerControl>() );
			}
		}
	}

	void startSprint( int level, PlayerControl playerControl )
	{
		playerControl.setAllowRunSpeedToIncrease( false );
		playerControl.runSpeed = playerControl.runSpeed * ( baseSpeed + level * speedUpgradePerLevel );
		playerControl.GetComponent<PlayerSounds>().playSound( soundFx, false );
		StartCoroutine( changeSprintBlendFactor( 0.85f, 0.8f, playerControl ) );
		StartCoroutine( stopSprint( level, playerControl ) );
	}

	IEnumerator stopSprint( int level, PlayerControl playerControl )
	{
		yield return new WaitForSeconds( baseDuration + level * durationUpgradePerLevel );
		if( playerControl.getCharacterState() != PlayerCharacterState.Dying ) playerControl.setAllowRunSpeedToIncrease( true );
		playerControl.GetComponent<PlayerSounds>().stopAudioSource();
		StartCoroutine( changeSprintBlendFactor( 0, 0.8f, playerControl ) );
	}

	IEnumerator changeSprintBlendFactor( float endBlendFactor, float duration, PlayerControl playerControl )
	{
		float elapsedTime = 0;
		float startBlendFactor = playerControl.getSprintBlendFactor();
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			playerControl.setSprintBlendFactor( Mathf.Lerp( startBlendFactor, endBlendFactor, elapsedTime/duration ) );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < duration );
		playerControl.setSprintBlendFactor( endBlendFactor );	
	}

}
