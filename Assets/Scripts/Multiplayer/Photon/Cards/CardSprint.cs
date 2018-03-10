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
		this.photonView.RPC("cardSprintMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardSprintMasterRPC( int level, int photonViewID )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		this.photonView.RPC("cardSprintRPC", PhotonTargets.AllViaServer, cd.getCardPropertyValue( CardPropertyType.DURATION, level ), cd.getCardPropertyValue( CardPropertyType.SPEED_MULTIPLIER, level ), photonViewID );	
	}
	#endregion

	[PunRPC]
	void cardSprintRPC( float spellDuration, float speedMultiplier, int photonViewID )
	{
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				startSprint( spellDuration, speedMultiplier, PlayerRace.players[i].GetComponent<PlayerRun>() );
			}
		}
	}

	void startSprint( float spellDuration, float speedMultiplier, PlayerRun playerRun )
	{
		playerRun.GetComponent<PlayerSounds>().playSound( soundFx, false );
		StartCoroutine( playerRun.addVariableSpeedMultiplier( SpeedMultiplierType.Sprint, speedMultiplier, 0.5f ) );
		StartCoroutine( stopSprint( spellDuration, playerRun ) );
	}

	IEnumerator stopSprint( float spellDuration, PlayerRun playerRun )
	{
		yield return new WaitForSeconds( spellDuration );
		playerRun.GetComponent<PlayerSounds>().stopAudioSource();
		StartCoroutine( playerRun.removeVariableSpeedMultiplier( SpeedMultiplierType.Sprint, 0.5f ) );
		playerRun.GetComponent<PlayerSpell>().cardDurationExpired( CardName.Sprint, false );
	}

}
