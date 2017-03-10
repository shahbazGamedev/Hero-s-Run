using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Shrink card is a Rare card with 11 levels.
/// The spell shrinks and slows the nearest player.
/// Because of its long range, it can be useful when an opponent is far away.
/// The spell range and duration depend on the level of the caster.
/// </summary>
public class CardShrink : Photon.PunBehaviour {

	[SerializeField] float  baseRange = 50f;
	[SerializeField] float  rangeUpgradePerLevel = 10f;
	[SerializeField] float  baseDuration = 5f;
	[SerializeField] float  durationUpgradePerLevel = 0.25f;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardShrinkMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardShrinkMasterRPC( int level, int photonViewID )
	{
		//Find out which player activated the card
		GameObject playerGameObject = null;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				playerGameObject = PlayerRace.players[i].gameObject;
			}
		}

		//Calculate spell range
		float spellRange = baseRange + level * rangeUpgradePerLevel;

		//Calculate spell duration
		float spellDuration = baseDuration + level * durationUpgradePerLevel;

		//Get nearest player
		Transform nearestTarget = detectNearestTarget( playerGameObject.transform, spellRange, photonViewID );

		if( nearestTarget != null )
		{
			nearestTarget.GetComponent<PhotonView>().RPC("shrinkSpell", PhotonTargets.All, spellDuration );
		}
		else
		{
			Debug.Log("CardShrink: no target found." );
		}
	}

	Transform detectNearestTarget( Transform caster, float spellRange, int photonViewID )
	{
		float nearestDistance = 100000;
		Transform nearestTarget = null;
		
		//Keep nearest target only
		for( int i =0; i < PlayerRace.players.Count; i++ )
		{
			//Ignore the caster
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID ) continue;

			//Calculate the distance to the other player
			float distanceToTarget = Vector3.Distance( caster.position, PlayerRace.players[i].transform.position );

			//Is this player within spell range?
			if( distanceToTarget > spellRange ) continue;

			//Is the player dead or Idle? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Dying || PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle ) continue;

			//Is it the closest player?
			if( distanceToTarget < nearestDistance )
			{
				nearestTarget = PlayerRace.players[i].transform;
				nearestDistance = distanceToTarget;
			}
		}
		return nearestTarget;
	}
	#endregion


}
