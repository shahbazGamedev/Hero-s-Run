﻿using UnityEngine;
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
	bool avalancheOccurred = false;

	void Awake ()
	{
		if( activationtype == ActivationType.PlayerDistance )
		{
			player = GameObject.FindGameObjectWithTag("Player").transform;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero")
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
			float  collapseDistance = distanceMultiplier * PlayerController.getPlayerSpeed();
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
		for( int i = 0; i < rigidBodies.Length; i++ )
		{
			rigidBodies[i].AddForce( forceToApply );
			rigidBodies[i].AddTorque( torqueToApply );
			Destroy( rigidBodies[i].gameObject, 10f );
		}
		GetComponent<AudioSource>().PlayDelayed( 0.3f );
	}

}
