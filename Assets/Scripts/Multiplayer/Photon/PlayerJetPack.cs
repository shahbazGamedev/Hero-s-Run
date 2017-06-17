using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerJetPack : MonoBehaviour {


	private float yawStrength = 4.5f;
	private float pitchStrength = 0.8f;
	private float rollStrength = 0.9f;
	private float verticalStrength = 10f;
	private float rotationDamping = 5f;
	//Used to apply a temporary speed boost diving down
	private static float speedBoost = 0f;
	private float pitchSpeedBoostStrength = 12f;

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
	CharacterController controller;
	#endregion

	#region Special Effects 	
	public Transform jetPack;
	public ParticleSystem exhaustRight;
	public ParticleSystem exhaustLeft;
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
		controller = GetComponent<CharacterController>();
		locateJetPack();
	}

	void locateJetPack()
	{
		//Try male prefab
		jetPack = transform.Find("Hero_prefab(Clone)/BASE_Master_Root/BASE_Root/BASE_Spine1/BASE_Spine2/BASE_Spine3/BASE_BackPack/Jet Pack");

		if( jetPack == null )
		{
			//Try female prefab
			jetPack = transform.Find("Heroine_prefab(Clone)/BASE_Master_Root/BASE_Root/BASE_Spine1/BASE_Spine2/BASE_Spine3/BASE_BackPack/Jet Pack");
		}

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
		Invoke("stopFlying", spellDuration );
		startFlying( flySpeed );
	}

	void startFlying( float flySpeed )
	{		
		this.flySpeed = flySpeed * LevelManager.Instance.speedOverrideMultiplier;
		playerControl.setCharacterState( PlayerCharacterState.Flying );
		GetComponent<Animator>().SetTrigger(FlyTrigger);
		playerSounds.stopAudioSource();
		//Clear move direction of any values. If we still have an x component for example, we will drift.
		moveDirection = Vector3.zero;
		accelerometerPreviousFrameX = 0;
		accelerometerPreviousFrameY = 0;
		playerCamera.setCameraParameters( 30f, 4f, 5.5f,0 );
		//Tilt the player's skin forward (superman style)
		GetComponent<PlayerVisuals>().heroSkin.transform.localRotation = Quaternion.Euler( 65f, 0, 0 );
		activateEffects( true );
		Debug.Log("startFlying" );
	}
	
	public void stopFlying( bool fall = true )
	{		
		if( playerControl.getCharacterState() == PlayerCharacterState.Flying || playerControl.getCharacterState() == PlayerCharacterState.Dying )
		{
			CancelInvoke( "stopFlying" );
			GetComponent<PlayerVisuals>().heroSkin.transform.localRotation = Quaternion.Euler( 0, 0, 0 );
			//If we stop flying because the timer expired, fall.
			//If we stop flying because we died, don't call fall since resurrect will do it.
			if( fall ) playerControl.fall();
			
			//Orient the player in the same direction as the current tile
			transform.rotation = Quaternion.Euler ( 0, playerControl.tileRotationY, 0 );
			
			activateEffects( false );

			playerCamera.resetCameraParameters();

			Debug.Log("stopFlying");
		}
	}

	void activateEffects( bool activate )
	{
		jetPack.gameObject.SetActive( activate );		
		exhaustRight.gameObject.SetActive( activate );		
		exhaustLeft.gameObject.SetActive( activate );		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if( GameManager.Instance.getGameState() == GameState.Normal )
		{
			#if UNITY_EDITOR
			handleKeyboard();
			#endif
			detectTaps();

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
		//8) Move the controller
		#if UNITY_EDITOR
		if( controller.enabled ) controller.Move( forward );
		#else
		controller.Move( forward );
		#endif
	}

/*
	void LateUpdate()
	{
		if( playerControl.getCharacterState() == PlayerCharacterState.Flying )
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
		}
	}
*/

	void detectTaps()
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
						stopFlying();
					}
				}
			}
		}
	}

	private void handleKeyboard()
	{
		if ( Input.GetKeyDown (KeyCode.F) ) 
		{
			if( playerControl.getCharacterState() != PlayerCharacterState.Flying )
			{
				startFlying( 28f );
			}
		}
		else if ( Input.GetKeyDown (KeyCode.A ) )
		{
			stopFlying();
		}
	}

}
