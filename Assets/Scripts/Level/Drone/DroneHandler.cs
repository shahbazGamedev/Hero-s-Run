using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneHandler : MonoBehaviour {

	[Header("Drone")]
	[SerializeField] float projectileSpeed = 1800;
    [SerializeField] Transform spawnPositionLeft;
    [SerializeField] Transform spawnPositionRight;
	[SerializeField] Sprite  minimapIcon;
	
	//Target
	public Transform nearestTarget = null;
	public PlayerControl nearestTargetPlayerControl = null;

	//Shooting related
	[Tooltip("The speed used by the drone when changing lanes to face its target.")]
	[SerializeField] float seekSpeed = 3.1f;
	[SerializeField] float weaponCoolDown = 0.9f;
	float timeOfLastShot;
	[SerializeField] float aimRange = 70f;

	Quaternion initialRotation;
	Vector3 initialPosition;

	#region Initialisation
	void Start()
	{
		initialRotation = transform.localRotation;
		initialPosition = transform.position;
		MiniMap.Instance.registerRadarObject( gameObject, minimapIcon );
	}
	#endregion

	#region Target detection and shooting
	void FixedUpdate()
	{
		//If the player died or is now in the Idle state (because of Stasis for example) or the player now has the Cloak card active, nullify the target
		if( nearestTargetPlayerControl != null && ( nearestTargetPlayerControl.getCharacterState() == PlayerCharacterState.Dying || nearestTargetPlayerControl.getCharacterState() == PlayerCharacterState.Idle || nearestTargetPlayerControl.GetComponent<PlayerSpell>().isCardActive(CardName.Cloak) ) )
		{
			resetNearestTarget();
		}

		//We don't want the drone to shoot while paused.
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
					object[] data = new object[5];
					data[0] = projectileSpeed;
					data[1] = nearestTarget.TransformPoint(0.2f,0,0);
					data[2] = nearestTarget.TransformPoint(-0.2f,0,0);
					data[3] = spawnPositionLeft.position;
					data[4] = spawnPositionRight.position;
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
				if( nearestTarget == null )
				{
					nearestTarget = other.transform;
					nearestTargetPlayerControl = nearestTarget.GetComponent<PlayerControl>();
					Invoke("resetNearestTarget", 3.5f );
				}
			}
		}
	}

	void resetNearestTarget()
	{
		nearestTarget = null;
		nearestTargetPlayerControl = null;
	}

	void OnTriggerExit(Collider other)
	{
		if( PhotonNetwork.isMasterClient )
		{
			if( other.transform == nearestTarget )
			{
				//The drone's target has left the target area.
				resetNearestTarget();
			}
		}
	}
}

