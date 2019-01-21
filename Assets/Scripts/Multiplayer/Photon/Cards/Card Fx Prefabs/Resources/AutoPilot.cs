using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPilot : Photon.PunBehaviour {

	[Header("General")]
	/// <summary>
	/// The card handler. We need access so that the bot can play cards from his deck.
	/// </summary>
	protected float percentageWillTryToAvoidObstacle;
	protected float percentageWillTurnSuccesfully;
	protected const float BASE_RUN_SPEED = 18f;
	protected const float BASE_OBSTACLE_DETECTION_LOW_DISTANCE = 6f; //assuming a run speed of BASE_RUN_SPEED
	protected const float BASE_OBSTACLE_DETECTION_HIGH_DISTANCE = 8f; //assuming a run speed of BASE_RUN_SPEED
	protected Vector3 xOffsetStartLow = new Vector3( 0, 0.5f, 0 );	//For low obstacles
	protected Vector3 xOffsetStartHigh = new Vector3( 0, 1.5f, 0 );	//For high obstacles

	protected PlayerControl playerControl;
	protected PlayerInput playerInput;
	protected PlayerRace playerRace;
	PlayerRun playerRun;

	// Use this for initialization
	protected void Awake ()
	{
		playerControl = GetComponent<PlayerControl>();
		playerInput = GetComponent<PlayerInput>();
		playerRace = GetComponent<PlayerRace>();
		playerRun = GetComponent<PlayerRun>();
	}

    //IMPORTANT: Queries Hit Triggers is set to false in the Physic settings.
    //This means that the raycast will NOT detect trigger colliders.
	protected void detectObstacles()
	{
		//Step 1) Adjust the obstacle detection range based on our run speed. If we are running fast, we need more time to react.
		float obstacleDetectionDistance = BASE_OBSTACLE_DETECTION_LOW_DISTANCE * playerRun.getRunSpeed()/BASE_RUN_SPEED;

		//Step 2) Detect if there are any low level obstacles
        RaycastHit hit;
		Vector3 exactPosStart = transform.TransformPoint( xOffsetStartLow );
		//Debug.DrawLine( exactPosStart, exactPosStart + transform.forward * obstacleDetectionDistance, Color.green );

        if (Physics.Raycast(exactPosStart, transform.forward, out hit, obstacleDetectionDistance ))
		{
			if( playerControl.getCharacterState() == PlayerCharacterState.Running )
			{
				//Debug.Log("detectObstacles LOW: " + hit.collider.name );
				if( hit.collider.name == "DeadTree" && shouldAvoidObstacle() )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Barrel" ) && shouldAvoidObstacle() )
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
				else if( hit.collider.CompareTag( "Chicken" ) && shouldAvoidObstacle() )
				{
					playerInput.startSlide();
				}
				else if( hit.collider.CompareTag( "Cart" ) && shouldAvoidObstacle() )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Cow" ) && shouldAvoidObstacle() )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Firewall" ) && shouldAvoidObstacle() )
				{
					//Did we cast this firewall?
					string caster = hit.collider.GetComponent<Firewall>().casterName;
					if( gameObject.name != caster )
					{
						//We did not cast it. We need to jump over it.
						//As a reminder, the caster is immune to the firewall he casted.
						playerInput.jump();
					}
				}
				else if( hit.collider.CompareTag( "Player" ) )
				{
					if( hit.collider.GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Dying )
					{
						//Jump over dead player
						playerInput.jump();
					}
				}
				else if( hit.collider.CompareTag( "Obstacle_B" ) )
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
				else if( hit.collider.CompareTag( "Obstacle_L" ) )
				{
                    playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Obstacle_S" ) )
				{
					playerInput.startSlide();
				}
				else if( hit.collider.CompareTag( "Obstacle_DJ" ) )
				{
					moveToCenterLane();
				}
				else if( hit.collider.CompareTag( "Zombie" ) )
				{
					handleZombieCollision( hit.collider.transform );
				}
			}
		}

		obstacleDetectionDistance = BASE_OBSTACLE_DETECTION_HIGH_DISTANCE * playerRun.getRunSpeed()/BASE_RUN_SPEED;
		exactPosStart = transform.TransformPoint( xOffsetStartHigh );
		//Debug.DrawLine( exactPosStart, exactPosStart + transform.forward * obstacleDetectionDistance, Color.yellow );
        if (Physics.Raycast(exactPosStart, transform.forward, out hit, obstacleDetectionDistance ))
		{
			if( playerControl.getCharacterState() == PlayerCharacterState.Running )
			{
				//Debug.Log("detectObstacles HIGH: " + hit.collider.name );
				if( hit.collider.name == "DeadTree" && shouldAvoidObstacle() )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Cart" ) && shouldAvoidObstacle() )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Cow" ) && shouldAvoidObstacle() )
				{
					playerInput.jump();
				}
				else if( hit.collider.CompareTag( "Firewall" ) && shouldAvoidObstacle() )
				{
					//Did we cast this firewall?
					string caster = hit.collider.GetComponent<Firewall>().casterName;
					if( gameObject.name != caster )
					{
						//We did not cast it. We need to jump over it.
						//As a reminder, the caster is immune to the firewall he casted.
						playerInput.jump();
					}
				}
				else if( hit.collider.CompareTag( "Obstacle_M" ) )
				{
					if( Random.value < 0.5f )
					{
						playerInput.startSlide();
					}
					else
					{
						playerInput.jump();
					}
				}
				else if( hit.collider.CompareTag( "Obstacle_S" ) )
				{
					playerInput.startSlide();
				}
			}
		}
	}

    bool shouldAvoidObstacle()
	{
		if (Random.value <= percentageWillTryToAvoidObstacle )
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	bool shouldTurnSuccessfully()
	{
		if (Random.value <= percentageWillTurnSuccesfully )
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	protected void OnTriggerEnter(Collider other)
	{
		//Carefull, if you turn right inside a deadEnd OnTriggerEnter will be called a second time (but not if your turn left).
		//This is probably a Unity bug.
		if( other.CompareTag( "deadEnd" ) && shouldTurnSuccessfully() )
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
		else if( other.CompareTag( "MoveToCenterLane" ) )
		{
			moveToCenterLane();
		}
		else if( other.CompareTag( "Jump" ) )
		{
			playerInput.jump();
		}
	}

	private void moveToCenterLane()
	{
		playerControl.recalculateCurrentLane();
		if( playerControl.currentLane == PlayerControl.Lanes.Left )
		{
			playerInput.sideSwipe( true );
		}
		else if( playerControl.currentLane == PlayerControl.Lanes.Right )
		{
			playerInput.sideSwipe( false );
		}
	}

	void handleZombieCollision( Transform collided )
	{
		ZombieController zombieController = collided.GetComponent<ZombieController>();

		if( zombieController.getCreatureState() == CreatureState.Crawling )
		{
			//The zombie is crawling. Jump over him.
			playerInput.jump();
		}
		else if( zombieController.getCreatureState() == CreatureState.BurrowUp )
		{
			//The zombie is burrowing up. Jump over him.
			playerInput.jump();
		}
		else
		{
			//The zombie is walking. Slide into it to make it fall.
			playerInput.startSlide();
		}
	}

}
