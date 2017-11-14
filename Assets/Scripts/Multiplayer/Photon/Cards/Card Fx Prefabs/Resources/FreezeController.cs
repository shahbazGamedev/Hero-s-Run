using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// See CardFreeze for details.
/// </summary>
public class FreezeController : CardSpawnedObject {

	[SerializeField] ParticleSystem tapParticleSystem;
	[SerializeField] AudioClip tapSound;
	[SerializeField] RFX4_StartDelay startDelayHandler;
	[SerializeField] GameObject ice;
	[SerializeField] GameObject iceGroundDecal;

	Transform affectedPlayerTransform;
	PlayerControl affectedPlayerControl;
	//if you tap quickly on the Stasis sphere, you can break free without waiting for the spell expires.
	int tapsDetected = 0;
	int tapsRequiredToBreak = 3;
	Coroutine destroyIceCoroutine;
	bool isLocalPlayer = false;
	float animSpeedAtTimeOfFreeze;

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
		findAffectedPlayer( gameObject.GetPhotonView ().instantiationData );
	}

	void findAffectedPlayer(object[] data) 
	{
		int viewIdOfAffectedPlayer = (int) data[0];
		tapsRequiredToBreak = (int) data[2];

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

				//isLocalPlayer is used by the code that allows a player to break out early by tapping on the stasis sphere.
				//It is used to ensure that you can only tap on the sphere the local player is trapped in.
				isLocalPlayer = ( affectedPlayerTransform.GetComponent<PhotonView>().isMine && affectedPlayerTransform.GetComponent<PlayerAI>() == null );
	

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

				//The Freeze has a limited lifespan which depends on the level of the Card.
				float spellDuration = (float) data[1];
				affectedPlayerTransform.GetComponent<PlayerSpell>().displayCardTimerOnHUD(CardName.Stasis, spellDuration );
				destroyIceCoroutine = StartCoroutine( destroyIce( spellDuration ) );
				startDelayHandler.Delay = spellDuration;

				//Display the Freeze secondary icon on the minimap
				MiniMap.Instance.displaySecondaryIcon( affectedPlayerTransform.GetComponent<PhotonView>().viewID, (int) CardName.Stasis, spellDuration );

				//Taps to break ice not implemented yet.
				//string tapInstructions = LocalizationManager.Instance.getText("CARD_TAP_INSTRUCTIONS");
				//if( isLocalPlayer ) HUDMultiplayer.hudMultiplayer.showTapInstructions( tapInstructions );

				//We can now make the ice visible and collidable
				//In order to detect taps/mouse-clicks properly, we need to change the layer to Default (it was Ignore Raycast).
				ice.layer = 0; //Default is 0
				ice.GetComponent<MeshCollider>().enabled = true;
				ice.GetComponent<MeshRenderer>().enabled = true;
				//Display the ground ice decal?
				bool displayGroundIceDecal = (bool) data[3];
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
	#endregion

	IEnumerator destroyIce( float delayBeforeSpellExpires )
	{
		yield return new WaitForSeconds(delayBeforeSpellExpires);
		destroyIceImmediately();
	}

	void destroyIceImmediately()
	{
		if( destroyIceCoroutine != null ) StopCoroutine( destroyIceCoroutine );
		//Taps to break ice not implemented yet.
		//if( isLocalPlayer ) HUDMultiplayer.hudMultiplayer.hideTapInstructions();
		MiniMap.Instance.hideSecondaryIcon( affectedPlayerTransform.gameObject );
		affectedPlayerTransform.GetComponent<Rigidbody>().isKinematic = false;
		affectedPlayerTransform.GetComponent<Animator>().speed = animSpeedAtTimeOfFreeze;
		//The player starts off running
		affectedPlayerTransform.GetComponent<Animator>().SetTrigger( "Run" );
		affectedPlayerTransform.GetComponent<PlayerControl>().setCharacterState( PlayerCharacterState.Running );

		affectedPlayerControl.enablePlayerMovement( true );
		affectedPlayerControl.enablePlayerControl( true );
		affectedPlayerTransform.GetComponent<PlayerSpell>().cardDurationExpired(CardName.Stasis);
		Destroy( gameObject );
	}

	//Taps to break ice not implemented yet.
	/*void Update () {

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
	}*/

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
				if( hit.collider.gameObject == ice )
				{
					tapsDetected++;
					ParticleSystem ps = GameObject.Instantiate( tapParticleSystem, hit.point, Quaternion.identity );
					Destroy( ps.gameObject, 2.5f );
					GetComponent<AudioSource>().PlayOneShot( tapSound );
					tapParticleSystem.Play();
					if( tapsDetected == tapsRequiredToBreak )
					{
						destroyIceImmediately();
					}
				}
			}
		}
	}
}
