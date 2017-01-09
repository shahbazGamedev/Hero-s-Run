using UnityEngine;
using System.Collections;

//Adds a small force and torque to the game objects with a rigid body as well as plays a sound.
[RequireComponent (typeof (AudioSource))]
public class PushRigidBodies : MonoBehaviour {

	public enum ActivationType {

		OnTriggerEnter = 0,
		PlayerDistance = 1
	}

	public ActivationType activationtype = ActivationType.OnTriggerEnter;
	public Vector3 forceToApply = new Vector3( 0, 20f, 120f );
	public Vector3 torqueToApply = new Vector3( 30f, 40f, 30f );
	public float distanceMultiplier = 1f;

	Transform player;
	PlayerController playerController;
	bool avalancheOccurred = false;

	void Start ()
	{
		if( activationtype == ActivationType.PlayerDistance )
		{
			player = GameObject.FindGameObjectWithTag("Player").transform;
			playerController = player.GetComponent<PlayerController>();
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if( other.gameObject.CompareTag("Player") )
		{
			startAvalanche();
		}
	}
	
	void Update ()
	{
		if( activationtype == ActivationType.PlayerDistance ) checkIfAvalanche();
	}

	void checkIfAvalanche()
	{
		if( !avalancheOccurred )
		{
			float distance = Vector3.Distance(player.position, transform.position);
			float  collapseDistance = distanceMultiplier * playerController.getSpeed();
			if( distance < collapseDistance  )
			{
				avalancheOccurred = true;
				startAvalanche();
			}
		}
	}

	void startAvalanche()
	{
		Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody>();
		Vector3 forces = forceToApply.z * transform.forward;
		forces.y = forceToApply.y;	
		for( int i = 0; i < rigidBodies.Length; i++ )
		{
			rigidBodies[i].isKinematic = false;
			rigidBodies[i].AddForce( forces );
			rigidBodies[i].AddTorque( torqueToApply );
			Destroy( rigidBodies[i].gameObject, 10f );
		}
		GetComponent<AudioSource>().PlayDelayed( 0.3f );
	}

}
