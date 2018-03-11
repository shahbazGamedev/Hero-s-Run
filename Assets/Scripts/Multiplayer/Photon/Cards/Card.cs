using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Card : Photon.PunBehaviour {

	public CardName  cardName;
	[Tooltip("The spawn offset is used to determine where to spawn the object relative to the caster. This value is ALSO used to determine if you are allowed to spawn an object in certain areas. For example, we would not want the player to spawn an object inside the finish line area.")]
	[SerializeField] Vector3 spawnOffset;
	[SerializeField]  Vector3 coopSpawnOffset;

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

	protected Vector3 getSpawnOffset()
	{
		if( GameManager.Instance.isCoopPlayMode() )
		{
			return coopSpawnOffset;
		}
		else
		{
			return spawnOffset;
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
		float nearestSqrDistance = Mathf.Infinity;
		//Keep nearest target only
		for( int i =0; i < PlayerRace.players.Count; i++ )
		{
			//Ignore the caster
			if( PlayerRace.players[i] == playerRace ) continue;

			//Calculate the square magnitude to the other player
			float sqrMagnitude = Vector3.SqrMagnitude( playerRace.transform.position - PlayerRace.players[i].transform.position );

			//Is this player within spell range?
			if( sqrMagnitude > ( spellRange * spellRange ) ) continue;

			//Is the player a valid target?
			if( !TargetManager.Instance.isPlayerValidTarget( PlayerRace.players[i].transform ) ) continue;

			//Is it the closest player?
			if( sqrMagnitude < nearestSqrDistance )
			{
				nearestTarget = PlayerRace.players[i].transform;
				nearestSqrDistance = sqrMagnitude;
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
	protected Transform detectBestTarget( PlayerRace playerRace, float range, bool useDotProduct = false )
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

			//Is the player a valid target?
			if( !TargetManager.Instance.isPlayerValidTarget( PlayerRace.players[i].transform ) ) continue;

			//If we are using the dot product, make sure that the target is in front.
			if( useDotProduct && !getDotProduct( playerRace.transform, PlayerRace.players[i].transform.position ) ) continue;

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

			//Calculate the square magnitude to the other player
			float sqrMagnitude = Vector3.SqrMagnitude( playerRace.transform.position - PlayerRace.players[i].transform.position );

			//Is this player within aiming range?
			if( sqrMagnitude > ( aimRange * aimRange ) ) continue;

			//Is the player a valid target?
			if( !TargetManager.Instance.isPlayerValidTarget( PlayerRace.players[i].transform ) ) continue;

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

	#region Multiple target detection
	protected List<ICreature> getAllCreaturesWithinRange( Transform caster, float range )
	{
		List<ICreature> creatures = new List<ICreature>();

		Collider[] hitColliders = Physics.OverlapSphere( caster.position, range, MaskHandler.getMaskOnlyCreatures() );
		ICreature creature;
		for( int i =0; i < hitColliders.Length; i++ )
		{
			//Is the target valid?
			if( !TargetManager.Instance.isCreatureTargetValid( hitColliders[i].transform, true, caster ) ) continue;

			creature = hitColliders[i].GetComponent<ICreature>();
			creatures.Add( creature );
		}
		return creatures;
	}

	protected List<Transform> getAllCreatureTransformsWithinRange( Transform caster, float range )
	{
		List<Transform> creatures = new List<Transform>();

		Collider[] hitColliders = Physics.OverlapSphere( caster.position, range, MaskHandler.getMaskOnlyCreatures() );
		Transform creature;
		for( int i =0; i < hitColliders.Length; i++ )
		{
			//Is the target valid?
			if( !TargetManager.Instance.isCreatureTargetValid( hitColliders[i].transform, true, caster ) ) continue;

			creature = hitColliders[i].transform;
			creatures.Add( creature );
		}
		return creatures;
	}
	#endregion


	#region Single target detection
	/// <summary>
	/// Gets the nearest creature within range that is in front of the caster.
	/// </summary>
	/// <returns>The nearest creature within range that is in front of the caster.</returns>
	/// <param name="caster">Caster.</param>
	/// <param name="range">Range.</param>
	protected Transform getNearestCreatureWithinRange( Transform caster, float range )
	{
		Transform nearestTarget;
		Collider[] hitColliders = Physics.OverlapSphere( caster.position, range, MaskHandler.getMaskOnlyCreatures() );
		nearestTarget = getNearestValidTarget( caster, hitColliders );
		return nearestTarget;
	}

	Transform getNearestValidTarget( Transform caster, Collider[] hitColliders )
	{
		Transform nearestTarget = null;
		float nearestDistance = Mathf.Infinity;
		for( int i =0; i < hitColliders.Length; i++ )
		{
			//Is the target valid?
			if( !TargetManager.Instance.isCreatureTargetValid( hitColliders[i].transform, true, caster ) ) continue;

			//Calculate the distance between this object and the potential target.
			float distanceToTarget = Vector3.Distance( transform.position, hitColliders[i].transform.position );

			//Is it the closest target?
			if( distanceToTarget < nearestDistance )
			{
				nearestTarget = hitColliders[i].transform;
				nearestDistance = distanceToTarget;
			}
		}
		if( nearestTarget != null ) Debug.Log( name + " getNearestValidTarget: " + nearestTarget.name );

		return nearestTarget;
	}
	#endregion

	/// <summary>
	/// Returns true if the target is in front of the caster, false otherwise.
	/// </summary>
	/// <returns><c>true</c>, if the target is in front of the caster, <c>false</c> otherwise.</returns>
	/// <param name="caster">Caster.</param>
	/// <param name="potentialTarget">Potential target.</param>
	protected bool getDotProduct( Transform caster, Transform target )
	{
		Vector3 forward = caster.TransformDirection(Vector3.forward);
		Vector3 toOther = target.position - caster.position;
		if (Vector3.Dot(forward, toOther) < 0)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	#region Voice Overs activated by Cards
	public void playActivateCardVoiceOver( PhotonView casterPhotonView )
	{
		//Play the activate card voice over but only if there is a VO available to play. We don't want to send RPCs for nothing and waste bandwidth.
		if( casterPhotonView.GetComponent<PlayerVoiceOvers>().isSpellVoiceOverAvailable( cardName, false ) ) casterPhotonView.RPC( "activateCardVoiceOverRPC", PhotonTargets.All, (int)cardName );
	}
	#endregion

}
