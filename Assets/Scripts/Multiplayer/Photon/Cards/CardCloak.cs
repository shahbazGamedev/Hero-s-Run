using System.Collections;
using UnityEngine;

public class CardCloak : Card {

	[SerializeField] AudioClip  soundFx;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardCloakMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardCloakMasterRPC( int level, int photonViewID )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		this.photonView.RPC("cardCloakRPC", PhotonTargets.AllViaServer, cd.getCardPropertyValue( CardPropertyType.DURATION_WITH_TIMER, level ), cd.getCardPropertyValue( CardPropertyType.SPEED_MULTIPLIER, level ), photonViewID );	
	}
	#endregion

	[PunRPC]
	void cardCloakRPC( float spellDuration, float speedMultiplier, int photonViewID )
	{
		Transform caster = getPlayerTransform( photonViewID );
		if( caster != null ) startCloaking( spellDuration, speedMultiplier, caster.GetComponent<PlayerRun>(), caster.GetComponent<PhotonView>().isMine, caster.GetComponent<PlayerAI>() == null );
	}

	void startCloaking( float spellDuration, float speedMultiplier, PlayerRun playerRun, bool isMine, bool isHuman )
	{
		playerRun.GetComponent<PlayerSounds>().playSound( soundFx, false );
		StartCoroutine( playerRun.addVariableSpeedMultiplier( SpeedMultiplierType.Cloak, speedMultiplier, 0.8f ) );
		StartCoroutine( stopCloaking( spellDuration, playerRun, isMine, isHuman ) );
 		playerRun.GetComponent<PlayerCamera>().setCameraProfile( PostProcessingProfileType.Cloak );

		//Hide the player's icon on the minimap
		MiniMap.Instance.changeAlphaOfRadarObject( playerRun.GetComponent<PlayerControl>(), 0 );
		//Make the online remote players invisible but not the local player.
		//A cloaked bot also becomes invisible.
		//Note that you will still be able to hear the cloaked player's footsteps.
		if( !isMine || !isHuman ) playerRun.GetComponent<PlayerSpell>().makePlayerInvisible();
	}

	IEnumerator stopCloaking( float spellDuration, PlayerRun playerRun, bool isMine, bool isHuman  )
	{
		yield return new WaitForSeconds( spellDuration );
		playerRun.GetComponent<PlayerSounds>().playSound( soundFx, false );
		StartCoroutine( playerRun.removeVariableSpeedMultiplier( SpeedMultiplierType.Cloak, 0.8f ) );
 		playerRun.GetComponent<PlayerCamera>().setCameraProfile( PostProcessingProfileType.Blank );

		MiniMap.Instance.changeAlphaOfRadarObject( playerRun.GetComponent<PlayerControl>(), 1f );
		if( !isMine || !isHuman )  playerRun.GetComponent<PlayerSpell>().makePlayerVisible();
		playerRun.GetComponent<PlayerSpell>().cardDurationExpired( CardName.Cloak, false );
	}
	
}
