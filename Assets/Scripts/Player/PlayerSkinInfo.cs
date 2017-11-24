﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrazyMinnow.SALSA;

public class PlayerSkinInfo : MonoBehaviour {
	
	public UnityEngine.Avatar animatorAvatar;
	public RuntimeAnimatorController runtimeAnimatorController;

	[Tooltip("The transform parent that owns the children that have the rigid bodies used by the ragdoll system.")]
	public Transform ragdollRigidBodyParent;
	[Tooltip("The Salsa component used for lip-sync. It will get copied to PlayerVoiceOvers when the player is instantiated.")]
	public Salsa3D headSalsa3D = null;

	//Not used by this class - here to avoid error message.
	public void punch_ground ( AnimationEvent eve )
	{
	}

	//Not used by this class - here to avoid error message.
	public void Lose_knees_hit_ground ( AnimationEvent eve )
	{
	}

}
