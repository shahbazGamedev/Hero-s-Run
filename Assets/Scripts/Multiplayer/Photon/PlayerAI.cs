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
public class PlayerAI : MonoBehaviour {

	const float OBSTACLE_DETECTION_DISTANCE = 3.3f;
	Vector3 xOffsetStartLow = new Vector3( 0, 0.5f, 0 );	//For low obstacles
	Vector3 xOffsetStartHigh = new Vector3( 0, 1.3f, 0 );	//For high obstacles

	PlayerControl playerControl;
	PlayerInput playerInput;

	// Use this for initialization
	void Awake ()
	{
		playerControl = GetComponent<PlayerControl>();
		playerInput = GetComponent<PlayerInput>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		detectObstacles();
	}

	void detectObstacles()
	{
        RaycastHit hit;
		Vector3 exactPosStart = transform.TransformPoint( xOffsetStartLow );
		//Debug.DrawLine( exactPosStart, exactPosStart + transform.forward * OBSTACLE_DETECTION_DISTANCE, Color.green );
        if (Physics.Raycast(exactPosStart, transform.forward, out hit, OBSTACLE_DETECTION_DISTANCE ))
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
					playerInput.startSlide();
				}
			}
		}

		exactPosStart = transform.TransformPoint( xOffsetStartHigh );
		//Debug.DrawLine( exactPosStart, exactPosStart + transform.forward * OBSTACLE_DETECTION_DISTANCE, Color.yellow );
        if (Physics.Raycast(exactPosStart, transform.forward, out hit, OBSTACLE_DETECTION_DISTANCE ))
		{
			if( playerControl.getCharacterState() == PlayerCharacterState.Running )
			{
				Debug.Log("detectObstacles HIGH: " + hit.collider.name );
				if( hit.collider.name == "DeadTree" )
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


}
