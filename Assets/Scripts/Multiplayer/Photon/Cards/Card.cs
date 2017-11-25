using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Card : Photon.PunBehaviour {

	public CardName  cardName;
	[Tooltip("The spawn offset is used to determine where to spawn the object relative to the caster. This value is ALSO used to determine if you are allowed to spawn an object in certain areas. For example, we would not want the player to spawn an object inside the finish line area.")]
	public Vector3 spawnOffset;

	protected PlayerControl getPlayerControl( int photonViewID )
	{
		PlayerControl playerControl = null;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				playerControl = PlayerRace.players[i].GetComponent<PlayerControl>();
				break;
			}
		}
		return playerControl;
	}

	protected Transform getPlayerTransform( int photonViewID )
	{
		Transform playerTransform = null;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				playerTransform = PlayerRace.players[i].transform;
				break;
			}
		}
		return playerTransform;
	}

	/// <summary>
	/// Returns true if the player is immune to spells.
	/// A player is immune when in the Idle or Dying state.
	/// Having the Cloak card active also makes the player immune.
	/// </summary>
	/// <returns><c>true</c>, if the player is immune, <c>false</c> otherwise.</returns>
	/// <param name="player">Player.</param>
	protected bool isPlayerImmune( Transform player  )
	{
		if( player.GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Idle && player.GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Dying )
		{
			if( player.GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) )
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			return true;
		}
	}
	
	/// <summary>
	/// Returns the transform of the nearest player within spell range.
	/// </summary>
	/// <returns>The nearest target.</returns>
	/// <param name="playerRace">Player race.</param>
	/// <param name="spellRange">Spell range.</param>
	protected Transform detectNearestTarget( PlayerRace playerRace, float spellRange )
	{
		Transform nearestTarget = null;
		float nearestDistance = 100000;
		//Keep nearest target only
		for( int i =0; i < PlayerRace.players.Count; i++ )
		{
			//Ignore the caster
			if( PlayerRace.players[i] == playerRace ) continue;

			//Calculate the distance to the other player
			float distanceToTarget = Vector3.Distance( playerRace.transform.position, PlayerRace.players[i].transform.position );

			//Is this player within spell range?
			if( distanceToTarget > spellRange ) continue;

			//Is the player dead or Idle? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerControl>().deathType != DeathType.Alive || PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle ) continue;

			//Is the player using the Cloak card? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) ) continue;

			//Is it the closest player?
			if( distanceToTarget < nearestDistance )
			{
				nearestTarget = PlayerRace.players[i].transform;
				nearestDistance = distanceToTarget;
			}
		}
		return nearestTarget;
	}

	/// <summary>
	/// Returns a player target or null if none were found within the range.
	/// If multiple targets are within range, it returns the player who is leading the most.
	/// </summary>
	/// <returns>A player target or null if none were found within the range.</returns>
	/// <param name="playerRace">Player race of the caster.</param>
	/// <param name="range">Range.</param>
	protected Transform detectBestTarget( PlayerRace playerRace, float range )
	{
		List<PlayerRace> potentialTargets = new List<PlayerRace>();
		float sqrRange = range * range;
		for( int i =0; i < PlayerRace.players.Count; i++ )
		{
			//Ignore the caster
			if( PlayerRace.players[i] == playerRace ) continue;

			//Calculate the distance to the other player
			float sqrMagnitude = Vector3.SqrMagnitude( playerRace.transform.position - PlayerRace.players[i].transform.position );

			//Is this player within range?
			if( sqrMagnitude > sqrRange ) continue;

			//Is the player dead or Idle or Ziplining? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerControl>().deathType != DeathType.Alive || PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle || PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Ziplining ) continue;

			//Is the player using the Cloak card? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) ) continue;

			//We have a potential target
			potentialTargets.Add( PlayerRace.players[i] );
		}
		
		if( potentialTargets.Count == 0 )
		{
			return null;
		}
		else if ( potentialTargets.Count == 1 )
		{
			return potentialTargets[0].transform;
		}
		else
		{
			//Privilege the player that has the lowest race position (i.e. the player leading the most)
			return potentialTargets.OrderBy( p => p.racePosition ).First().transform;			
		}
	}

	/// <summary>
	/// Gets the dot product.
	/// </summary>
	/// <returns><c>true</c>, if the other player is in front of the caster, <c>false</c> otherwise.</returns>
	/// <param name="caster">Caster.</param>
	/// <param name="otherPlayerPosition">The other player's position.</param>
	bool getDotProduct( Transform caster, Vector3 otherPlayerPosition )
	{
		Vector3 forward = caster.TransformDirection(Vector3.forward);
		Vector3 toOther = otherPlayerPosition - caster.position;
		if (Vector3.Dot(forward, toOther) < 0)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	/// <summary>
	/// Returns true if there is a player target within spell range.
	/// </summary>
	/// <returns><c>true</c>, if there is a player target within range, <c>false</c> otherwise.</returns>
	/// <param name="caster">Caster.</param>
	/// <param name="level">Level.</param>
	public bool isThereATargetWithinRange( Transform caster, int level )
	{
		//See if there is a target
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		Transform nearestTarget = detectNearestTarget( caster.GetComponent<PlayerRace>(), cd.getCardPropertyValue( CardPropertyType.RANGE, level ) );

		return nearestTarget != null;
	}

	public bool isTargetInRange( PlayerRace playerRace, float aimRange )
	{
		for( int i =0; i < PlayerRace.players.Count; i++ )
		{
			//Ignore the potential sentry owner
			if( PlayerRace.players[i] == playerRace ) continue;

			//Calculate the distance to the other player
			float distanceToTarget = Vector3.Distance( playerRace.transform.position, PlayerRace.players[i].transform.position );

			//Is this player within aiming range?
			if( distanceToTarget > aimRange ) continue;

			//Is the player dead or Idle? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerControl>().deathType != DeathType.Alive || PlayerRace.players[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle ) continue;

			//Is the player using the Cloak card? If so, ignore.
			if( PlayerRace.players[i].GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) ) continue;

			//We found at least one target
			return true;
		}
		return false;
	}

	public bool isAllowed( int photonViewID )
	{
		//Find out which player activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Verify that the player is not spawning the object inside the finish line trigger
		Vector3 objectPosition = playerTransform.TransformPoint( spawnOffset );

		GameObject finishLineColliderObject = GameObject.FindGameObjectWithTag("Finish Line");
		//If the tile with the finish line is not active, boxColliderObject will be null, so check for that.
		if( finishLineColliderObject != null )
		{
			Collider collider = finishLineColliderObject.GetComponent<Collider>();
			if( collider.bounds.Contains( objectPosition ) )
			{
				//Don't allow it
				return false;
			}
			else
			{
				return true;
			}
		}
		else
		{
			//If finishLineColliderObject is null, that means the tile with the finish line is not yet active and therefore, we are far from the finish line.
			//In this case, return true.
			return true;
		}
	}

	/// <summary>
	/// Returns true if an opponent is within the radius specified by this card.
	/// </summary>
	/// <returns><c>true</c>, if an opponent is near, <c>false</c> otherwise.</returns>
	/// <param name="caster">Caster.</param>
	/// <param name="level">Level.</param>
	public bool isOpponentNear( Transform caster, int level )
	{
		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		float impactRadius = cd.getCardPropertyValue( CardPropertyType.RADIUS, level );
		Collider[] hitColliders = Physics.OverlapSphere( caster.position, impactRadius, MaskHandler.getMaskOnlyPlayer() );
		for( int i =0; i < hitColliders.Length; i++ )
		{
			//Verify that it is NOT the caster activating the card
			if( hitColliders[i].name != caster.name )
			{
				//Verify that the target is not in the Idle character state.
				//The target will be in the Idle state after crossing the finish line for example.
				//Make the sure the player is not dead.
				//Make sure the player does not have the Cloak card active.
				if( hitColliders[i].GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Idle && hitColliders[i].GetComponent<PlayerControl>().getCharacterState() != PlayerCharacterState.Dying )
				{
					if( !hitColliders[i].GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) )
					{
						//We found at least one target
						return true;
					}
				}
			}
		}
		//No target were found
		return false;
	}

	#region Target detection
	protected Transform getNearestCreatureWithinRange( Transform caster, float range, int mask )
	{
		Transform nearestTarget;
		Collider[] hitColliders = Physics.OverlapSphere( caster.position, range, mask );
		nearestTarget = getNearestValidTarget( hitColliders );
		return nearestTarget;
	}

	Transform getNearestValidTarget( Collider[] hitColliders )
	{
		Transform nearestTarget = null;
		float nearestDistance = Mathf.Infinity;
		for( int i =0; i < hitColliders.Length; i++ )
		{
			//Is the target valid?
			if( !isTargetValid( hitColliders[i].transform ) ) continue;

			//Calculate the distance between this object and the potential target.
			float distanceToTarget = Vector3.Distance( transform.position, hitColliders[i].transform.position );

			//Is it the closest target?
			if( distanceToTarget < nearestDistance )
			{
				nearestTarget = hitColliders[i].transform;
				nearestDistance = distanceToTarget;
			}
		}
		return nearestTarget;
	}

	bool isTargetValid( Transform potentialTarget )
	{
		bool valid = false;
   		switch (potentialTarget.gameObject.layer)
		{
	        case MaskHandler.creatureLayer:
				ICreature creatureController = potentialTarget.GetComponent<ICreature>();
				if( creatureController != null && creatureController.getCreatureState() != CreatureState.Dying && creatureController.getCreatureState() != CreatureState.Immobilized )
				{
					valid = true;
				}
                break;
		}
		if( valid ) Debug.Log("isTargetValid " + potentialTarget.name );
		return valid;
	}
	#endregion

	#region Voice Overs activated by Cards
	public void playActivateCardVoiceOver( PhotonView casterPhotonView )
	{
		//Play the activate card voice over but only if there is a VO available to play. We don't want to send RPCs for nothing and waste bandwidth.
		if( casterPhotonView.GetComponent<PlayerVoiceOvers>().isSpellVoiceOverAvailable( cardName, false ) ) casterPhotonView.RPC( "activateCardVoiceOverRPC", PhotonTargets.All, (int)cardName );
	}
	#endregion
}
