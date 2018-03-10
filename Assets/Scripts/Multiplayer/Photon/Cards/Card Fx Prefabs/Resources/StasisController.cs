using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// See CardStasis for details.
/// </summary>
public class StasisController : CardSpawnedObject {

	[Header("Tap to break free")]
	//if you tap quickly on the Stasis sphere, you can break free without waiting for the spell expires.
	[SerializeField] ParticleSystem tapParticleSystem; //Put the stop action to Destroy
	[SerializeField] AudioClip tapSound;
	int tapsDetected = 0;
	int tapsRequiredToBreakStasis;
	bool isLocalPlayer = false;

	[Header("Player")]
	Transform affectedPlayerTransform;
	PlayerControl affectedPlayerControl;
	Coroutine destroyStasisSphereCoroutine;

	[Header("Creature")]
	Transform affectedCreatureTransform;
	Coroutine destroyStasisSphereCreatureCoroutine;

	[Header("Other")]
	[SerializeField] float y_pos_player_in_sphere = -0.45f;

	#region Initialisation
	void OnPhotonInstantiate( PhotonMessageInfo info )
	{
		//Note that the Stasis sphere prefab has its MeshRenderer and SphereCollider disabled.
		//We will enable them when the card gets activated by the lockstep manager.
		LockstepManager.LockstepAction lsa = new LockstepManager.LockstepAction( LockstepActionType.CARD, gameObject, CardName.Stasis );
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
				zombies[i].stasis( true );

				affectedCreatureTransform.position = transform.TransformPoint( new Vector3( 0, y_pos_player_in_sphere, 0 ) );

				//The Stasis Sphere has a limited lifespan which depends on the level of the Card.
				float spellDuration = (float) data[1];
				destroyStasisSphereCreatureCoroutine = StartCoroutine( destroyStasisSphereCreature( spellDuration ) );

				//We can now make the sphere visible and collidable
				//In order to detect taps/mouse-clicks properly, we need to change the layer to Default (it was Ignore Raycast).
				gameObject.layer = 0; //Default is 0
				GetComponent<SphereCollider>().enabled = true;
				GetComponent<MeshRenderer>().enabled = true;
				break;
			}
		}
		if( affectedCreatureTransform != null )
		{
			Debug.Log("Stasis-The creature affected by the Stasis Sphere is: " + affectedCreatureTransform.name );
		}
		else
		{
			Debug.LogError("StasisController error: could not find the target creature with the Photon view id of " + viewIdOfAffectedCreature );
		}
	}

	IEnumerator destroyStasisSphereCreature( float delayBeforeSpellExpires )
	{
		yield return new WaitForSeconds(delayBeforeSpellExpires);
		destroyStasisSphereImmediatelyCreature();
	}

	void destroyStasisSphereImmediatelyCreature()
	{
		if( destroyStasisSphereCreatureCoroutine != null ) StopCoroutine( destroyStasisSphereCreatureCoroutine );
		affectedCreatureTransform.GetComponent<ZombieController>().stasis( false );
		Destroy( gameObject );
	}
	#endregion

	#region Player
	void findAffectedPlayer(object[] data) 
	{
		int viewIdOfAffectedPlayer = (int) data[0];
		tapsRequiredToBreakStasis = (int) data[2];

		for( int i = 0; i < PlayerRace.players.Count; i ++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == viewIdOfAffectedPlayer )
			{
				//We found the spell's target
				affectedPlayerTransform = PlayerRace.players[i].transform;
				affectedPlayerControl = affectedPlayerTransform.GetComponent<PlayerControl>();

				//If in the short time between the card being cast and the card being activated
				//the player has died or is IDLE, simply ignore.
				if( affectedPlayerControl.getCharacterState() == PlayerCharacterState.Dying || affectedPlayerControl.getCharacterState() == PlayerCharacterState.Idle ) return;

				affectedPlayerTransform.GetComponent<Rigidbody>().isKinematic = true;

				//isLocalPlayer is used by the code that allows a player to break out early by tapping on the stasis sphere.
				//It is used to ensure that you can only tap on the sphere the local player is trapped in.
				isLocalPlayer = ( affectedPlayerTransform.GetComponent<PhotonView>().isMine && affectedPlayerTransform.GetComponent<PlayerAI>() == null );
	

				//If the player is affected by shrink, cancel it. The player will enlarge back to his normal size.
				affectedPlayerTransform.GetComponent<PlayerSpell>().cancelShrinkSpell();

				//Freeze the player's movement and remove player control.
				affectedPlayerControl.enablePlayerMovement( false );
				affectedPlayerControl.enablePlayerControl( false );

				affectedPlayerTransform.position = transform.TransformPoint( new Vector3( 0, y_pos_player_in_sphere, 0 ) );

				affectedPlayerTransform.GetComponent<Animator>().SetTrigger( "Stasis" );
				//Set the player state to Idle so that other spells don't affect the player while he is in Statis.
				affectedPlayerControl.setCharacterState( PlayerCharacterState.Idle );

				//If the player has a Sentry, it will be destroyed.
				affectedPlayerTransform.GetComponent<PlayerSpell>().cancelSentrySpell();

				//The Stasis Sphere has a limited lifespan which depends on the level of the Card.
				float spellDuration = (float) data[1];
				affectedPlayerTransform.GetComponent<PlayerSpell>().displayCardTimerOnHUD(CardName.Stasis, spellDuration );
				destroyStasisSphereCoroutine = StartCoroutine( destroyStasisSphere( spellDuration ) );

				//Display the Stasis secondary icon on the minimap
				MiniMap.Instance.displaySecondaryIcon( affectedPlayerTransform.GetComponent<PhotonView>().viewID, (int) CardName.Stasis, spellDuration );

				string tapInstructions = LocalizationManager.Instance.getText("CARD_TAP_INSTRUCTIONS");
				if( isLocalPlayer ) HUDMultiplayer.hudMultiplayer.showTapInstructions( tapInstructions );

				//We can now make the sphere visible and collidable
				//In order to detect taps/mouse-clicks properly, we need to change the layer to Default (it was Ignore Raycast).
				gameObject.layer = 0; //Default is 0
				GetComponent<SphereCollider>().enabled = true;
				GetComponent<MeshRenderer>().enabled = true;
				break;
			}
		}
		if( affectedPlayerTransform != null )
		{
			Debug.Log("Stasis-The player affected by the Stasis Sphere is: " + affectedPlayerTransform.name );
		}
		else
		{
			Debug.LogError("StasisController error: could not find the target player with the Photon view id of " + viewIdOfAffectedPlayer );
		}
	}

	IEnumerator destroyStasisSphere( float delayBeforeSpellExpires )
	{
		yield return new WaitForSeconds(delayBeforeSpellExpires);
		destroyStasisSphereImmediately();
	}

	void destroyStasisSphereImmediately()
	{
		if( destroyStasisSphereCoroutine != null ) StopCoroutine( destroyStasisSphereCoroutine );
		if( isLocalPlayer ) HUDMultiplayer.hudMultiplayer.hideTapInstructions();
		MiniMap.Instance.hideSecondaryIcon( affectedPlayerTransform.gameObject );
		affectedPlayerTransform.GetComponent<Rigidbody>().isKinematic = false;
		affectedPlayerControl.fall();
		affectedPlayerControl.enablePlayerMovement( true );
		affectedPlayerControl.enablePlayerControl( true );
		affectedPlayerTransform.GetComponent<PlayerSpell>().cardDurationExpired( CardName.Stasis, true );
		Destroy( gameObject );
	}

	void Update () {

		if( !isLocalPlayer ) return;

		#if UNITY_EDITOR
		// User pressed the left mouse up
		if (Input.GetMouseButtonDown(0))
		{
			MouseButtonDown(0);
		}
		#else
		detectTaps();
		#endif
	}

	void MouseButtonDown(int Button)
	{
		validateTappedObject(Input.mousePosition);
	}

	void detectTaps()
	{
		for ( int i = 0; i < Input.touchCount; i++ )
		{
			if( Input.GetTouch(i).phase == TouchPhase.Began  )
			{
				validateTappedObject(Input.GetTouch(i).position);
			}
		}
	}

	void validateTappedObject( Vector2 touchPosition )
	{
		// We need to actually hit an object
		RaycastHit hit;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(touchPosition), out hit, 10))
		{
			if ( hit.collider )
			{
				if( hit.collider.gameObject == gameObject )
				{
					tapsDetected++;
					ParticleSystem ps = GameObject.Instantiate( tapParticleSystem, hit.point, Quaternion.identity );
					Destroy( ps.gameObject, 2.5f );
					GetComponent<AudioSource>().PlayOneShot( tapSound );
					tapParticleSystem.Play();
					if( tapsDetected == tapsRequiredToBreakStasis )
					{
						destroyStasisSphereImmediately();
					}
				}
			}
		}
	}
	#endregion
}
