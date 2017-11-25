using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// See CardFreeze for details.
/// </summary>
public class FreezeController : CardSpawnedObject {

	[SerializeField] GameObject ice;
	[SerializeField] GameObject iceGroundDecal;

	//For player
	Transform affectedPlayerTransform;
	PlayerControl affectedPlayerControl;
	Coroutine destroyIceCoroutine;
	float animSpeedAtTimeOfFreeze;

	//For creatures
	Transform affectedCreatureTransform;
	Coroutine destroyIceCreatureCoroutine;

	#region Initialisation
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Note that the Freeze prefab has its MeshRenderer and MeshCollider disabled.
		//We will enable them when the card gets activated by the lockstep manager.
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.CARD, gameObject, CardName.Freeze );
		lsa.cardSpawnedObject = this;
		LockstepManager.Instance.addActionToQueue( lsa );
	}

	public override void activateCard()
	{
		if( GameManager.Instance.isCoopPlayMode() )
		{
			findAffectedCreature( gameObject.GetPhotonView ().instantiationData );
		}
		else
		{
			findAffectedPlayer( gameObject.GetPhotonView ().instantiationData );
		}
	}
	#endregion

	#region Player
	void findAffectedPlayer(object[] data) 
	{
		int viewIdOfAffectedPlayer = (int) data[0];

		for( int i = 0; i < PlayerRace.players.Count; i ++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == viewIdOfAffectedPlayer )
			{
				//We found the spell's target
				affectedPlayerTransform = PlayerRace.players[i].transform;
				affectedPlayerControl = affectedPlayerTransform.GetComponent<PlayerControl>();

				//If in the short time between the card being cast and the card being activated
				//the player has died or is IDLE, simply ignore.
				if( affectedPlayerControl.deathType != DeathType.Alive || affectedPlayerControl.getCharacterState() == PlayerCharacterState.Idle ) return;

				affectedPlayerTransform.GetComponent<Rigidbody>().isKinematic = true;

				//If the player is affected by shrink, cancel it. The player will enlarge back to his normal size.
				affectedPlayerTransform.GetComponent<PlayerSpell>().cancelShrinkSpell();

				//Freeze the player's movement and remove player control.
				affectedPlayerControl.enablePlayerMovement( false );
				affectedPlayerControl.enablePlayerControl( false );

				affectedPlayerTransform.position = transform.position;

				animSpeedAtTimeOfFreeze = affectedPlayerTransform.GetComponent<Animator>().speed;
				affectedPlayerTransform.GetComponent<Animator>().speed = 0;
				//Set the player state to Idle so that other spells don't affect the player while he is in Statis.
				affectedPlayerControl.setCharacterState( PlayerCharacterState.Idle );

				//If the player has a Sentry, it will be destroyed.
				affectedPlayerTransform.GetComponent<PlayerSpell>().cancelSentrySpell();

				//The Freeze has a limited lifespan.
				float spellDuration =  6.5f;

				affectedPlayerTransform.GetComponent<PlayerSpell>().displayCardTimerOnHUD(CardName.Freeze, spellDuration );
				destroyIceCoroutine = StartCoroutine( destroyIce( spellDuration ) );

				//Display the Freeze secondary icon on the minimap
				MiniMap.Instance.displaySecondaryIcon( affectedPlayerTransform.GetComponent<PhotonView>().viewID, (int) CardName.Freeze, spellDuration );

				//We can now make the ice visible and collidable
				//In order to detect taps/mouse-clicks properly, we need to change the layer to Default (it was Ignore Raycast).
				ice.layer = 0; //Default is 0
				ice.GetComponent<MeshCollider>().enabled = true;
				ice.GetComponent<MeshRenderer>().enabled = true;
				//Display the ground ice decal?
				bool displayGroundIceDecal = (bool) data[1];
				iceGroundDecal.GetComponent<MeshRenderer>().enabled = displayGroundIceDecal; 
				break;
			}
		}
		if( affectedPlayerTransform != null )
		{
			Debug.Log("FreezeController-The player affected by the Freeze is: " + affectedPlayerTransform.name );
		}
		else
		{
			Debug.LogError("FreezeController error: could not find the target player with the Photon view id of " + viewIdOfAffectedPlayer );
		}
	}

	IEnumerator destroyIce( float delayBeforeSpellExpires )
	{
		yield return new WaitForSeconds(delayBeforeSpellExpires);
		destroyIceImmediately();
	}

	void destroyIceImmediately()
	{
		if( destroyIceCoroutine != null ) StopCoroutine( destroyIceCoroutine );
		MiniMap.Instance.hideSecondaryIcon( affectedPlayerTransform.gameObject );
		affectedPlayerTransform.GetComponent<Rigidbody>().isKinematic = false;
		affectedPlayerTransform.GetComponent<Animator>().speed = animSpeedAtTimeOfFreeze;
		//The player starts off running
		affectedPlayerTransform.GetComponent<Animator>().SetTrigger( "Run" );
		affectedPlayerTransform.GetComponent<PlayerControl>().setCharacterState( PlayerCharacterState.Running );

		affectedPlayerControl.enablePlayerMovement( true );
		affectedPlayerControl.enablePlayerControl( true );
		affectedPlayerTransform.GetComponent<PlayerSpell>().cardDurationExpired(CardName.Freeze);
		Destroy( gameObject );
	}
	#endregion

	#region Creature
	void findAffectedCreature(object[] data) 
	{
		int viewIdOfAffectedCreature = (int) data[0];

		ZombieController[] zombies = GameObject.FindObjectsOfType<ZombieController>();
		for( int i = 0; i < zombies.Length; i ++ )
		{
			if( zombies[i].GetComponent<PhotonView>().viewID == viewIdOfAffectedCreature )
			{
				//We found the spell's target
				affectedCreatureTransform = zombies[i].transform;

				//If in the short time between the card being cast and the card being activated
				//the creature has died, simply ignore.
				if( zombies[i].getCreatureState() == CreatureState.Dying  ) return;

				//Freeze the creature's movement.
				zombies[i].immobilize( true );

				affectedCreatureTransform.position = transform.position;

				//The Freeze has a limited lifespan.
				float spellDuration =  6.5f;

				destroyIceCreatureCoroutine = StartCoroutine( destroyIceCreature( spellDuration ) );

				//We can now make the ice visible and collidable
				//In order to detect taps/mouse-clicks properly, we need to change the layer to Default (it was Ignore Raycast).
				ice.layer = 0; //Default is 0
				ice.GetComponent<MeshCollider>().enabled = true;
				ice.GetComponent<MeshRenderer>().enabled = true;
				//Display the ground ice decal?
				bool displayGroundIceDecal = (bool) data[1];
				iceGroundDecal.GetComponent<MeshRenderer>().enabled = displayGroundIceDecal; 
				break;
			}
		}
		if( affectedCreatureTransform != null )
		{
			Debug.Log("FreezeController-The creature affected by the Freeze is: " + affectedCreatureTransform.name );
		}
		else
		{
			Debug.LogError("FreezeController error: could not find the target creature with the Photon view id of " + viewIdOfAffectedCreature );
		}
	}

	IEnumerator destroyIceCreature( float delayBeforeSpellExpires )
	{
		yield return new WaitForSeconds(delayBeforeSpellExpires);
		destroyIceImmediatelyCreature();
	}

	void destroyIceImmediatelyCreature()
	{
		if( destroyIceCreatureCoroutine != null ) StopCoroutine( destroyIceCreatureCoroutine );
		
		affectedCreatureTransform.GetComponent<ZombieController>().immobilize( false );
		Destroy( gameObject );
	}
	#endregion
}
