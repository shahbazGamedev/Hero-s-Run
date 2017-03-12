using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

/// <summary>
//// The Speed Boost card is a Rare card with 11 levels.
/// </summary>
public class CardSpeedBoost : Photon.PunBehaviour {

	[SerializeField] AudioClip  soundFx;
	[SerializeField] float  baseDuration = 1.5f;
	[SerializeField] float  baseSpeed = 1.5f;
	[SerializeField] float  durationUpgradePerLevel = 0.15f;
	[SerializeField] float  speedUpgradePerLevel = 0.06f;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardSpeedBoostRPC", PhotonTargets.AllViaServer, level, photonViewId );	
	}

	[PunRPC]
	void cardSpeedBoostRPC( int level, int photonViewID )
	{
		string casterName = string.Empty;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				startSpeedBoost( level, PlayerRace.players[i].GetComponent<PlayerControl>(), PlayerRace.players[i].GetComponent<PhotonView>().isMine && PlayerRace.players[i].GetComponent<PlayerAI>() == null );
				casterName = PlayerRace.players[i].name;
			}
		}
		//Indicate on the minimap which card was played
		MiniMap.Instance.updateCardFeed( casterName, CardName.Raging_Bull );
	}

	void startSpeedBoost( int level, PlayerControl playerControl, bool isMine )
	{
		//Only affect the camera for the local player
		if( isMine ) Camera.main.GetComponent<MotionBlur>().enabled = true;
		playerControl.setAllowRunSpeedToIncrease( false );
		playerControl.isSpeedBoostActive = true;
		playerControl.runSpeed = playerControl.runSpeed * ( baseSpeed + level * speedUpgradePerLevel );
		playerControl.GetComponent<PlayerSounds>().playSound( soundFx, false );
		StartCoroutine( changeSprintBlendFactor( 1f, 0.7f, playerControl ) );
		StartCoroutine( stopSpeedBoost( level, playerControl, isMine) );
	}

	IEnumerator stopSpeedBoost( int level, PlayerControl playerControl, bool isMine )
	{
		yield return new WaitForSeconds( baseDuration + level * durationUpgradePerLevel );
		if( isMine ) Camera.main.GetComponent<MotionBlur>().enabled = false;
		if( playerControl.getCharacterState() != PlayerCharacterState.Dying ) playerControl.setAllowRunSpeedToIncrease( true );
		playerControl.GetComponent<PlayerSounds>().stopAudioSource();
		playerControl.isSpeedBoostActive = false;
		StartCoroutine( changeSprintBlendFactor( 0, 0.7f, playerControl ) );
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
