using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerJetPack : Photon.PunBehaviour {


	private float yawStrength = 4.5f;
	private float pitchStrength = 0.8f;
	private float rollStrength = 0.9f;
	private float verticalStrength = 10f;
	private float rotationDamping = 5f;
	//Used to apply a temporary speed boost diving down
	private static float speedBoost = 0f;
	private float pitchSpeedBoostStrength = 12f;
	const float FLY_SPEED_MULTIPLIER = 60f; //or else you just don't fly fast enough

	//For accelerometer
	float accelerometerPreviousFrameX = 0;
	float accelerometerPreviousFrameY = 0;
	float accelerometerStrength = 14f;

	public Vector3 moveDirection;
	float flySpeed;
	int FlyTrigger = Animator.StringToHash("Fly");
	const float MAXIMUM_YAW = 30f;

	#region Other variables
	GenerateLevel generateLevel;
	#endregion

	#region Components cached for performance 	
	PlayerControl playerControl;
	PlayerSounds playerSounds;
	PlayerCamera playerCamera;
	Rigidbody rigidbody;
	#endregion

	#region Special Effects 	
	public Transform heroSkin;
	public Transform jetPack;
	public ParticleSystem exhaustRight;
	public ParticleSystem exhaustLeft;
	#endregion

	#region Movement sync
	float timeOfLastSync;
	const float DELAY_BETWEEN_SYNC = 0.1f; //in seconds
	#endregion

	void Awake ()
	{
		generateLevel = GameObject.FindObjectOfType<GenerateLevel>();
	}

	// Use this for initialization
	void Start ()
	{
		playerControl = GetComponent<PlayerControl>();
		playerSounds = GetComponent<PlayerSounds>();
		playerCamera = GetComponent<PlayerCamera>();
		rigidbody = GetComponent<Rigidbody>();
	}

	void locateJetPack()
	{
		if( heroSkin != null ) return;

		heroSkin = transform.Find("Hero Skin");

		jetPack = transform.Find("Hero Skin/BASE_Master_Root/BASE_Root/BASE_Spine1/BASE_Spine2/BASE_Spine3/BASE_BackPack/Jet Pack");

		if( jetPack != null )
		{
			exhaustLeft = jetPack.Find("Left Exhaust/Flames").GetComponent<ParticleSystem>(); 
			exhaustRight = jetPack.Find("Right Exhaust/Flames").GetComponent<ParticleSystem>(); 
		}
		else
		{
			Debug.LogError("PlayerJetPack-locateJetPack error: unable to find jet pack. Verify the transform hierarchy.");
		}
	}
	
	[PunRPC]
	void cardJetPackRPC( float spellDuration, float flySpeed )
	{
		//Display the Jet Pack secondary icon on the minimap
		MiniMap.Instance.displaySecondaryIcon( GetComponent<PhotonView>().viewID, (int) CardName.Jet_Pack, spellDuration );
		//Cancel the Jet Pack effect once the duration has run out
		Invoke("durationHasExpired", spellDuration );
		startFlying( flySpeed );
	}

	void startFlying( float flySpeed )
	{		
		locateJetPack();
		this.flySpeed = flySpeed * LevelManager.Instance.speedOverrideMultiplier;
		playerControl.setCharacterState( PlayerCharacterState.Flying );
		GetComponent<Rigidbody>().useGravity = false;
		GetComponent<Animator>().SetTrigger(FlyTrigger);
		playerSounds.stopAudioSource();
		//Clear move direction of any values. If we still have an x component for example, we will drift.
		moveDirection = Vector3.zero;
		accelerometerPreviousFrameX = 0;
		accelerometerPreviousFrameY = 0;
		playerCamera.setCameraParameters( 30f, 4f, 5.5f,0 );
		//Tilt the player's skin forward (superman style)
		heroSkin.transform.localRotation = Quaternion.Euler( 65f, 0, 0 );
		activateEffects( true );
		Debug.Log("startFlying" );
	}
	
	void durationHasExpired()
	{		
		stopFlying( true );
	}

	public void stopFlying( bool fall )
	{		
		if( playerControl.getCharacterState() == PlayerCharacterState.Flying || playerControl.getCharacterState() == PlayerCharacterState.Dying )
		{
			CancelInvoke( "durationHasExpired" );
			if( heroSkin != null ) heroSkin.transform.localRotation = Quaternion.Euler( 0, 0, 0 );
			//If we stop flying because the timer expired, fall.
			//If we stop flying because we died, don't call fall since resurrect will do it.
			GetComponent<Rigidbody>().useGravity = true;
			if( fall ) playerControl.fall( true );
			
			//Orient the player in the same direction as the current tile
			transform.rotation = Quaternion.Euler ( 0, playerControl.tileRotationY, 0 );
			
			activateEffects( false );

			playerCamera.resetCameraParameters();

			Debug.Log("stopFlying");
		}
	}

	void activateEffects( bool activate )
	{
		if( jetPack == null ) return;
		jetPack.gameObject.SetActive( activate );		
		exhaustRight.gameObject.SetActive( activate );		
		exhaustLeft.gameObject.SetActive( activate );		
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		if( GameManager.Instance.getGameState() == GameState.Normal )
		{
			#if UNITY_EDITOR
			handleKeyboard();
			#endif
			detectTaps();

			if( PhotonNetwork.isMasterClient )
			{
				if( playerControl.getCharacterState() == PlayerCharacterState.Flying )
				{
					if( playerControl.isPlayerControlEnabled() )
					{
						//Reset value
						speedBoost = 0;
						//Section A - adjust accelerometer values
						float accelerometerCurrentFrameX = Input.acceleration.x;
						//Notes:
						//if the device is horizontal, accelerometerCurrentFrameY = 0
						//if the device is vertical with the home button at the bottom, accelerometerCurrentFrameY = 1
						//if the device is vertical with the home button at the top, accelerometerCurrentFrameY = -1
						float accelerometerCurrentFrameY = -Input.acceleration.y;
						//If the player holds the phone tilted up at an angle of 45 degrees (accelerometerCurrentFrameY = 0.5f) we
						//want to stay flat; if the phone is tilted more, we want to go up; if the phone is tilted at less, we want to go down.
						//If the phone is really tilted down (i.e. accelerometerCurrentFrameY is less then zero), we cap the value.
						if( accelerometerCurrentFrameY < 0 ) accelerometerCurrentFrameY = 0;
						
						//Should we add a speed boost due to pitch?
						if( accelerometerCurrentFrameY < 0.65f )
						{
							//We are doing a nose dive, boost the speed.
							speedBoost = ( 0.65f - accelerometerCurrentFrameY ) * pitchSpeedBoostStrength;
						}
						//verticalStrength controls how fast we move up and down as well as pitch
						accelerometerCurrentFrameY = -( 0.65f - accelerometerCurrentFrameY ) * verticalStrength;
						
						//Section B - rotations
						//Note: transform.eulerAngles always returns a value between 0 and 360 degrees.
						
						//1 - Calculate Roll - Z axis
						//Z axis is counter-clockwise, so +10 degrees tilts left and 350 degrees tilts right.
						float roll;
						if( Mathf.Abs ( accelerometerCurrentFrameX ) > 0.2f )
						{	
							//The player is tilting the device sufficiently, so tilt
							roll = transform.eulerAngles.z - accelerometerCurrentFrameX * rollStrength;
							roll = Utilities.clampRotation ( roll, 15f );
						}
						else
						{
							//The player is not tilting the device a lot so slowly go back to 0 degrees.
							roll = Mathf.LerpAngle (transform.eulerAngles.z, 0, rotationDamping * Time.deltaTime);
						}
							
						//2 - Calculate Pitch - X axis				
						float pitch = transform.eulerAngles.x - accelerometerCurrentFrameY * pitchStrength;
						//X axis is counter-clockwise, so +10 degrees tilts forward and 350 degrees tilts backward.
						pitch = Utilities.clampRotation ( pitch, 30f );
						
						//3 - Calculate Yaw - Y axis				
						float yaw = transform.eulerAngles.y + accelerometerCurrentFrameX * yawStrength;
						//Do not allow player to fly back to where he started. He must always move forward.
					//yaw = Utilities.clampRotation ( yaw, MAXIMUM_YAW );
	
						//4 - Rotate
						transform.rotation = Quaternion.Euler ( pitch, yaw, roll );
						
						//Section C - translations
						accelerometerCurrentFrameY = accelerometerCurrentFrameY * 1.5f;
						moveDirection.Set(  moveDirection.x , moveDirection.y + accelerometerCurrentFrameY - accelerometerPreviousFrameY, moveDirection.z );
						accelerometerPreviousFrameY = accelerometerCurrentFrameY;
					
						fly();
					}
				}
			}
		}
	}

	void fly()
	{
		//1) Get the direction of the player
		Vector3 forward = transform.TransformDirection(Vector3.forward);			
		//2) Scale vector based on run speed
		forward = forward * Time.deltaTime * flySpeed;
		//3) Add Y component for gravity. Both the x and y components are stored in moveDirection.
		forward.Set( forward.x, moveDirection.y * Time.deltaTime, forward.z );
		//4) Get a unit vector that is orthogonal to the direction of the player
		Vector3 relativePos = new Vector3(1 , 0 , 0 );
		Vector3 xPos = transform.TransformPoint(relativePos);
		Vector3 xVector = xPos - transform.position;
		//5) Scale the X component
		xVector = xVector * Time.deltaTime * moveDirection.x;
		//6) Clamp to the max distance we can travel perpendicularly without
		//exiting the left or right lanes.
		//xVector =  Vector3.ClampMagnitude(xVector, Mathf.Abs(getMaxDist(moveDirection.x)));
		//7) Add the X component to the forward direction
		forward = forward + xVector;
		//8) Move
		rigidbody.velocity = forward * FLY_SPEED_MULTIPLIER;
	}


	void LateUpdate()
	{
		/*if( playerControl.getCharacterState() == PlayerCharacterState.Flying )
		{
			//Make sure the player remains above a certain height
			if( transform.position.y < -0.6f )
			{
				transform.position = new Vector3 (  transform.position.x, -0.6f, transform.position.z );
			}
			//Make sure the player never goes above a certain height
			else if( transform.position.y > 60f )
			{
				transform.position = new Vector3 (  transform.position.x, 60f, transform.position.z );
			}
		}*/
		if( playerControl.getCharacterState() == PlayerCharacterState.Flying && PhotonNetwork.isMasterClient && ( Time.time - timeOfLastSync) > DELAY_BETWEEN_SYNC )
		{
			this.photonView.RPC("flySyncRPC", PhotonTargets.Others, transform.position, transform.eulerAngles, PhotonNetwork.time );
			timeOfLastSync = Time.time;
		}
	}

	[PunRPC]
	void flySyncRPC( Vector3 syncPosition, Vector3 syncRotation, double timeRPCSent )
	{
		//Use the values we received from the master
		transform.eulerAngles = syncRotation;
		transform.position = syncPosition;
		//There was a delay between the master sending us the command and the remote receiving it.
		//Predict where the player should be and move him there before executing the command.
		float syncTimeDelta = (float)(PhotonNetwork.time - timeRPCSent);
		//1) Get the direction of the player
		Vector3 forward = transform.TransformDirection(Vector3.forward);			
		//2) Scale vector based on flight speed
		rigidbody.velocity = forward * syncTimeDelta * flySpeed * FLY_SPEED_MULTIPLIER;
		
		//The master player and the remote player should now be synchronised.
	}

	#region For debugging Jet Pack. Normal way to activate is by playing the Jet Pack card.
	void detectTaps()
	{
		if( this.photonView.isMine && GetComponent<PlayerAI>() == null && PlayerStatsManager.Instance.getShowDebugInfoOnHUD() )
		{			
			if ( Input.touchCount > 0 )
			{
				Touch touch = Input.GetTouch(0);
				if( touch.tapCount == 2 )
				{
					if( touch.phase == TouchPhase.Ended  )
					{
						if( playerControl.getCharacterState() != PlayerCharacterState.Flying )
						{
							startFlying( 28f );
						}
						else
						{
							stopFlying( true );
						}
					}
				}
			}
		}
	}

	private void handleKeyboard()
	{
		if( this.photonView.isMine && GetComponent<PlayerAI>() == null && PlayerStatsManager.Instance.getShowDebugInfoOnHUD() )
		{			
			if ( Input.GetKeyDown (KeyCode.F) ) 
			{
				if( playerControl.getCharacterState() != PlayerCharacterState.Flying )
				{
					startFlying( 28f );
				}
				else
				{
					stopFlying( true );
				}
			}
		}
	}
	#endregion
}
