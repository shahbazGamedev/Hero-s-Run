using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneHandler : MonoBehaviour {

	[Header("Drone")]
	[SerializeField] float projectileSpeed = 1000;
    [SerializeField] Transform spawnPositionLeft;
    [SerializeField] Transform spawnPositionRight;
	
	//Target
	public Transform nearestTarget = null;

	//Shooting related
	[SerializeField] float seekSpeed = 3f;
	[SerializeField] float weaponCoolDown = 2f;
	float timeOfLastShot;
	[SerializeField] float aimRange = 70f;

	Quaternion initialRotation;
	Vector3 initialPosition;

	#region Initialisation
	void Start()
	{
		initialRotation = transform.localRotation;
		initialPosition = transform.position;
	}
	#endregion

	#region Target detection and shooting
	void FixedUpdate()
	{
		//We don't want the sentry to shoot while paused.
		//Remember that in multiplayer the time scale is not set to 0 while paused.
		if( GameManager.Instance.getGameState() == GameState.Normal )
		{
			lookAtTarget();
		}
	}

	void lookAtTarget()
	{
		if( nearestTarget != null )
		{
			//The drone has a target. Change lane to face it at seekSpeed.
			float nearestTargetRotationY = Mathf.Floor( nearestTarget.eulerAngles.y );
			Vector3 targetPosition;
			if( nearestTargetRotationY == 0 )
			{
				//Target is running South-North. Use X.
				targetPosition = new Vector3( nearestTarget.position.x, transform.position.y, transform.position.z );
			}
			else
			{
				//Target is running West-East. Use Z.
				targetPosition = new Vector3( transform.position.x, transform.position.y, nearestTarget.position.z );
			}
			transform.position = Vector3.MoveTowards( transform.position, targetPosition, Time.deltaTime * seekSpeed );

			//Now orient the drone so that it aims towards the player's torso
			transform.LookAt(nearestTarget.TransformPoint( 0, 1.2f, 0 ));
			//Cap the X angle
			float xAngle = Mathf.Min( transform.eulerAngles.x, 30f );
			transform.localRotation = Quaternion.Euler (xAngle, initialRotation.eulerAngles.y, 0 );

			//Verify if we can hit the nearest target
			shoot();
 		}
		else
		{
			//The drone doesn't have a target. Go back to the initial position and rotation.
			transform.localRotation = Quaternion.Lerp( transform.localRotation, initialRotation, Time.deltaTime * seekSpeed );
			transform.position = Vector3.MoveTowards( transform.position, initialPosition, Time.deltaTime * seekSpeed );
		}
	}

	void Update () 
	{
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
			shoot();
        }
	}

	void shoot()
	{
		if( nearestTarget == null ) return;

		if( Time.time - timeOfLastShot > weaponCoolDown )
		{
			//Verify if we can hit the nearest target
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.forward, out hit, aimRange ))
			{
				if( hit.collider.transform == nearestTarget )
				{
					timeOfLastShot = Time.time;

					//Create projectiles
					object[] data = new object[6];
					data[0] = projectileSpeed;
					data[1] = aimRange;
					data[2] = nearestTarget.TransformPoint(0.2f,1.2f,0);
					data[3] = nearestTarget.TransformPoint(-0.2f,1.2f,0);
					data[4] = spawnPositionLeft.position;
					data[5] = spawnPositionRight.position;
					PhotonNetwork.InstantiateSceneObject( "Drone Projectiles", transform.position, transform.rotation, 0, data );
				}
			}
		}
	}
	
	#endregion

	void OnTriggerEnter(Collider other)
	{
		if( PhotonNetwork.isMasterClient )
		{
			if( other.CompareTag( "Player" ) )
			{
				if( nearestTarget == null ) nearestTarget = other.transform;
			}
		}
	}

	void OnTriggerExit(Collider other)
	{
		if( PhotonNetwork.isMasterClient )
		{
			if( other.CompareTag( "Player" ) )
			{
				nearestTarget = null;
			}
		}
	}
}

