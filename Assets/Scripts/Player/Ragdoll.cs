using UnityEngine;
using System.Collections;
 
public class Ragdoll : MonoBehaviour {
  
Component[] ragdollRigidBodies;
Animator anim;
CapsuleCollider capsuleCollider;

	public void initializeRagdoll ( Animator anim, Transform ragdollRigidBodyParent, CapsuleCollider capsuleCollider )
	{
		ragdollRigidBodies = ragdollRigidBodyParent.GetComponentsInChildren<Rigidbody> (); 
		this.anim = anim;
		this.capsuleCollider = capsuleCollider;
		if( ragdollRigidBodies != null ) Debug.Log("initializeRagdoll-number of bones: " + ragdollRigidBodies.Length + " for " + capsuleCollider.name );
	}

	public void controlRagdoll ( bool enableRagdoll ) 
	{
		if( ragdollRigidBodies == null ) return;
		foreach (Rigidbody ragdoll in ragdollRigidBodies)
		{
			ragdoll.isKinematic = !enableRagdoll;
			ragdoll.detectCollisions = enableRagdoll;

		}
	
		anim.enabled = !enableRagdoll;
		//Clean-up
		//capsuleCollider.enabled = !enableRagdoll;
	}
		
}
