using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
//// The Sprint card is a Common card with 13 levels.
/// </summary>
public class CardSprint : Card {

	[SerializeField] AudioClip  soundFx;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardSprintMasterRPC", PhotonTargets.AllViaServer, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardSprintMasterRPC( int level, int photonViewID )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		this.photonView.RPC("cardSprintRPC", PhotonTargets.All, cd.getCardPropertyValue( CardPropertyType.DURATION, level ), cd.getCardPropertyValue( CardPropertyType.RUN_SPEED, level ), photonViewID );	
	}
	#endregion

	[PunRPC]
	void cardSprintRPC( float spellDuration, float speedMultiplier, int photonViewID )
	{
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				startSprint( spellDuration, speedMultiplier, PlayerRace.players[i].GetComponent<PlayerControl>() );
			}
		}
	}

	void startSprint( float spellDuration, float speedMultiplier, PlayerControl playerControl )
	{
		playerControl.setAllowRunSpeedToIncrease( false );
		playerControl.runSpeed = playerControl.runSpeed * speedMultiplier;
		playerControl.GetComponent<PlayerSounds>().playSound( soundFx, false );
		StartCoroutine( changeSprintBlendFactor( 0.85f, 0.8f, playerControl ) );
		StartCoroutine( stopSprint( spellDuration, playerControl ) );
	}

	IEnumerator stopSprint( float spellDuration, PlayerControl playerControl )
	{
		yield return new WaitForSeconds( spellDuration );
		if( playerControl.getCharacterState() != PlayerCharacterState.Dying ) playerControl.setAllowRunSpeedToIncrease( true );
		playerControl.GetComponent<PlayerSounds>().stopAudioSource();
		StartCoroutine( changeSprintBlendFactor( 0, 0.8f, playerControl ) );
	}

}
