using UnityEngine;
using System.Collections;

public class animationEventDispatcher : MonoBehaviour {

	PlayerController playerController;

	// Use this for initialization
	void Awake () {
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		playerController = player.GetComponent<PlayerController>();

	}

	public void death_completed ( AnimationEvent eve )
	{
		playerController.death_completed( eve );
	}

	public void stumble_completed ( AnimationEvent eve )
	{
		playerController.stumble_completed( eve );
	}

	public void get_up_completed ( AnimationEvent eve )
	{
		playerController.get_up_completed( eve );
	}

	public void Footstep_right ( AnimationEvent eve )
	{
		playerController.Footstep_right( eve );
	}

	public void Footstep_left ( AnimationEvent eve )
	{
		playerController.Footstep_left( eve );
	}

	public void Land_sound ( AnimationEvent eve )
	{
		playerController.Land_sound( eve );
	}

	public void Teleport_leave_complete ( AnimationEvent eve )
	{
		playerController.teleportLeaveComplete();
	}

}
