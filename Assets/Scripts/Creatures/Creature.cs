using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Creature : BaseClass {

	protected CreatureState creatureState = CreatureState.Idle;
	[Header("Other")]
	protected Transform player;
	protected CharacterController controller;
	protected Animator anim;

	protected void Awake ()
	{
		controller = GetComponent<CharacterController>();
		player = GameObject.FindGameObjectWithTag("Player").transform;
		anim = GetComponent<Animator>();
	}

	public CreatureState getCreatureState()
	{
		return creatureState;
	}

	public void setCreatureState( CreatureState state )
	{
		creatureState = state;
	}

	/*
		returns:
		-1 if creature is behind player
		+1 if creature is in front
		0 if creature is on the side
		0.5 if creature is facing player and within 60 degrees (i.e. between 30 degrees to the left and 30 degrees to the right)
	*/
	protected float getDotProduct()
	{
		Vector3 heading = player.position - transform.position;
		return Vector3.Dot( heading.normalized, transform.forward );
	}

}
