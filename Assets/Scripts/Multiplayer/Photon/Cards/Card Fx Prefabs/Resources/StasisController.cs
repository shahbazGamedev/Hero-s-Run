using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// See CardStasis for details.
/// </summary>
public class StasisController : CardSpawnedObject {

	[SerializeField] ParticleSystem tapParticleSystem;
	[SerializeField] AudioClip tapSound;
	Transform affectedPlayerTransform;
	PlayerControl affectedPlayerControl;
	const float DISTANCE_ABOVE_GROUND = 3.5f;
	const float Y_POS_PLAYER_IN_SPHERE = -0.35f;
	//if you tap quickly on the Stasis sphere, you can break free without waiting for the spell expires.
	int tapsDetected = 0;
	int tapsRequiredToBreakStasis = 3;
	Coroutine destroyStasisSphereCoroutine;
	bool isLocalPlayer = false;

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
		findAffectedPlayer( gameObject.GetPhotonView ().instantiationData );
	}

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
				affectedPlayerTransform.GetComponent<Rigidbody>().isKinematic = true;

				//isLocalPlayer is used by the code that allows a player to break out early by tapping on the stasis sphere.
				//It is used to ensure that you can only tap on the sphere the local player is trapped in.
				isLocalPlayer = ( affectedPlayerTransform.GetComponent<PhotonView>().isMine && affectedPlayerTransform.GetComponent<PlayerAI>() == null );
	
				affectedPlayerControl = affectedPlayerTransform.GetComponent<PlayerControl>();

				//If the player was ziplining when he got affected by stasis, detach him from the zipline.
				affectedPlayerControl.detachFromZipline();

				//If the player is affected by shrink, cancel it. The player will enlarge back to his normal size.
				affectedPlayerTransform.GetComponent<PlayerSpell>().cancelShrinkSpell();

				//Freeze the player's movement and remove player control.
				affectedPlayerControl.enablePlayerMovement( false );
				affectedPlayerControl.enablePlayerControl( false );

				//We want the statis sphere to float DISTANCE_ABOVE_GROUND above ground.
				//We add 0.1f because we do not want to start the raycast at the player's feet.
				//The Stasis prefab has the ignoreRaycast layer.
				affectedPlayerControl.gameObject.layer = MaskHandler.ignoreRaycastLayer; //change temporarily to Ignore Raycast
				RaycastHit hit;
				if (Physics.Raycast(new Vector3( affectedPlayerTransform.position.x, affectedPlayerTransform.position.y + 0.1f, affectedPlayerTransform.position.z ), Vector3.down, out hit, 30.0F ))
				{
					transform.position = new Vector3( transform.position.x, hit.point.y + DISTANCE_ABOVE_GROUND, transform.position.z );
				}
				else
				{
					Debug.LogWarning("StasisController-there is no ground below the affected player: " + affectedPlayerControl.name );
				}
				affectedPlayerTransform.position = transform.TransformPoint( new Vector3( 0, Y_POS_PLAYER_IN_SPHERE, 0 ) );
				affectedPlayerTransform.gameObject.layer = MaskHandler.playerLayer; //Restore to Player
				//Slow down the anim speed to give the impression of the player being stuck
				affectedPlayerTransform.GetComponent<Animator>().speed = 0.3f;
				affectedPlayerTransform.GetComponent<Animator>().Play( "Fall_Loop" );
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

				string tapInstructions = LocalizationManager.Instance.getText("CARD_STASIS_TAP_INSTRUCTIONS");
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
	#endregion

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
		affectedPlayerTransform.GetComponent<Animator>().speed = 1f;
		affectedPlayerControl.fall( true );
		affectedPlayerControl.enablePlayerMovement( true );
		affectedPlayerControl.enablePlayerControl( true );
		affectedPlayerTransform.GetComponent<PlayerSpell>().cardDurationExpired(CardName.Stasis);
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
		if ( Input.touchCount > 0 )
		{
			Touch touch = Input.GetTouch(0);
			if( touch.tapCount == 1 )
			{
				if( touch.phase == TouchPhase.Began  )
				{
					validateTappedObject(Input.GetTouch(0).position);
				}
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
}
