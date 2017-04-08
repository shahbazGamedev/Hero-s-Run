using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Stasis card is a Common card with 13 levels.
/// The spell traps the player in a stasis force field for the spell duration, which depends on the level of the card.
/// The player is immune to other spells while in stasis.
/// The spell range depends on the level of the card.
/// </summary>
public class CardStasis : Card {

	[SerializeField] float  baseDuration = 3f;
	[SerializeField] float  durationUpgradePerLevel = 0.5f;
	[SerializeField] float  baseRange = 100f;
	[SerializeField] float  rangeUpgradePerLevel = 10f;
	[SerializeField]  string prefabName;

	public void activateCard ( int photonViewId, int level )
	{
		this.photonView.RPC("cardStasisMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardStasisMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Calculate the spell range
		float spellRange = baseRange + level * rangeUpgradePerLevel;

		//Find the nearest target
		Transform nearestTarget = detectNearestTarget( playerTransform.GetComponent<PlayerRace>(), spellRange );

		//Only continue if we found a target
		if( nearestTarget != null )
		{
			object[] data = new object[2];
	
			//We will need to find a reference to the player we are targeting
			data[0] = nearestTarget.GetComponent<PhotonView>().viewID;
	
			//We want the stasis sphere to disappear after a while
			data[1] = baseDuration + level * durationUpgradePerLevel;
	
			PhotonNetwork.InstantiateSceneObject( prefabName, nearestTarget.position, nearestTarget.rotation, 0, data );
		}
		else
		{
			Debug.Log("CardStasis - No target found.");
		}
	}
	#endregion

	Transform detectNearestTarget( PlayerRace playerRace, float spellRange )
	{
		Transform nearestTarget = null;
		float nearestDistance = 100000;
		//Keep nearest target only
		for( int i =0; i < PlayerRace.players.Count; i++ )
		{
			//Ignore the caster
			if( PlayerRace.players[i] == playerRace ) continue;

			//Calculate the distance to the other player
			float distanceToTarget = Vector3.Distance( transform.position, PlayerRace.players[i].transform.position );

			//Is this player within spell range?
			if( distanceToTarget > spellRange ) continue;

			//Is the player dead or Idle? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerControl>().deathType != DeathType.Alive || PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle ) continue;

			//Is it the closest player?
			if( distanceToTarget < nearestDistance )
			{
				nearestTarget = PlayerRace.players[i].transform;
				nearestDistance = distanceToTarget;
			}
		}
		return nearestTarget;
	}

	public bool willSpellBeEffective( Transform caster, int level )
	{
		//Calculate the spell range
		float spellRange = baseRange + level * rangeUpgradePerLevel;

		//See if there is a target
		Transform nearestTarget = detectNearestTarget( caster.GetComponent<PlayerRace>(), spellRange );

		return nearestTarget != null;
	}
}
