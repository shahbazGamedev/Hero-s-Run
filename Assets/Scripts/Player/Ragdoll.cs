using UnityEngine;
using System.Collections;
 
public class Ragdoll : MonoBehaviour {
  
	[SerializeField] bool enableRagdoll = false;
	Component[] ragdollRigidBodies;
	Animator anim;
	CapsuleCollider capsuleCollider;

	public void initializeRagdoll ( Animator anim, Transform ragdollRigidBodyParent, CapsuleCollider capsuleCollider )
	{
		if( !enableRagdoll ) return;
		ragdollRigidBodies = ragdollRigidBodyParent.GetComponentsInChildren<Rigidbody> (); 
		this.anim = anim;
		this.capsuleCollider = capsuleCollider;
		if( ragdollRigidBodies != null ) Debug.Log("initializeRagdoll-number of bones: " + ragdollRigidBodies.Length + " for " + capsuleCollider.name );
	}

	public void controlRagdoll ( bool enable ) 
	{
		if( !enableRagdoll ) return;
		if( ragdollRigidBodies == null ) return;
		foreach (Rigidbody ragdoll in ragdollRigidBodies)
		{
			ragdoll.isKinematic = !enable;
			ragdoll.detectCollisions = enable;

		}
	
		anim.enabled = !enable;
		//Clean-up
	}
}
