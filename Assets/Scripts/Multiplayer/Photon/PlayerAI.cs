using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player AI. Important this component MUST be placed just below the PlayerControl component to work properly.
/// Currently allows AI character to:
/// 1) automatically turn corners
/// 2) break barrels
/// 3) jump over low obstacles and high obstacles.
/// </summary>
public class PlayerAI : Photon.PunBehaviour {

	[Header("General")]
	/// <summary>
	/// The card handler. We need access so that the bot can play cards from his deck.
	/// </summary>
	CardHandler cardHandler;
	const float BASE_RUN_SPEED = 18f;
	const float BASE_OBSTACLE_DETECTION_LOW_DISTANCE = 4.6f; //assuming a run speed of BASE_RUN_SPEED
	const float BASE_OBSTACLE_DETECTION_HIGH_DISTANCE = 8f; //assuming a run speed of BASE_RUN_SPEED
	Vector3 xOffsetStartLow = new Vector3( 0, 0.5f, 0 );	//For low obstacles
	Vector3 xOffsetStartHigh = new Vector3( 0, 1.3f, 0 );	//For high obstacles

	PlayerControl playerControl;
	PlayerInput playerInput;

	// Use this for initialization
	void Awake ()
	{
		playerControl = GetComponent<PlayerControl>();
		playerInput = GetComponent<PlayerInput>();
		cardHandler = GameObject.FindGameObjectWithTag("Card Handler").GetComponent<CardHandler>();
		Invoke("doSpeedBoost",30f);
	}
	
	void doSpeedBoost()
	{
		if( playerControl.getCharacterState() == PlayerCharacterState.Running )
		{
			cardHandler.activateCard( this.photonView.viewID, "Barbarians", 2 );
		}
	}

	// Update is called once per frame
	void Update ()
	{
		#if UNITY_EDITOR
		handleKeyboard();
		#endif
		detectObstacles();
	}

	void detectObstacles()
	{
		//Step 1) Adjust the obstacle detection range based on our run speed. If we are running fast, we need more time to react.
		float obstacleDetectionDistance = BASE_OBSTACLE_DETECTION_LOW_DISTANCE * playerControl.getSpeed()/BASE_RUN_SPEED;

		//Step 2) Detect if there are any low level obstacles
        RaycastHit hit;
		Vector3 exactPosStart = transform.TransformPoint( xOffsetStartLow );
		//Debug.DrawLine( exactPosStart, exactPosStart + transform.forward * obstacleDetectionDistance, Color.green );

        if (Physics.Raycast(exactPosStart, transform.forward, out hit, obstacleDetectionDistance ))
		{
			if( playerControl.getCharacterState() == PlayerCharacterState.Running )
			{
				Debug.Log("detectObstacles LOW: " + hit.collider.name );
				if( hit.collider.name == "DeadTree" )
				{
					playerInput.jump();
				}
				else if( hit.collider.name.StartsWith( "Breakable Barrel" ) )
				{
					if( Random.value < 0.8f )
					{
						playerInput.startSlide();
					}
					else
					{
						playerInput.jump();
					}
				}
				else if( hit.collider.CompareTag( "Chicken" ) )
				{
					playerInput.startSlide();
				}
				else if( hit.collider.CompareTag( "Cart" ) )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Cow" ) )
				{
					playerInput.jump();
				}
			}
		}

		obstacleDetectionDistance = BASE_OBSTACLE_DETECTION_HIGH_DISTANCE * playerControl.getSpeed()/BASE_RUN_SPEED;
		exactPosStart = transform.TransformPoint( xOffsetStartHigh );
		//Debug.DrawLine( exactPosStart, exactPosStart + transform.forward * obstacleDetectionDistance, Color.yellow );
        if (Physics.Raycast(exactPosStart, transform.forward, out hit, obstacleDetectionDistance ))
		{
			if( playerControl.getCharacterState() == PlayerCharacterState.Running )
			{
				Debug.Log("detectObstacles HIGH: " + hit.collider.name );
				if( hit.collider.name == "DeadTree" )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Cart" ) )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Cow" ) )
				{
					playerInput.jump();
				}
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
		//Carefull, if you turn right inside a deadEnd OnTriggerEnter will be called a second time (but not if your turn left).
		//This is probably a Unity bug.
		if( other.name == "deadEnd" )
		{
			DeadEndType currentDeadEndType = other.GetComponent<deadEnd>().deadEndType;
			if ( currentDeadEndType == DeadEndType.Left )
			{
				playerInput.sideSwipe( false );
			}
			else if ( currentDeadEndType == DeadEndType.Right )
			{
				playerInput.sideSwipe( true );
			}
		}
	}

	private void handleKeyboard()
	{
		if ( Input.GetKeyDown (KeyCode.B ) )
		{
			//Kill bot for testing
			playerControl.managePlayerDeath(DeathType.FallForward);
		}
	}


}
