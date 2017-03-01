using UnityEngine;
using System.Collections;
 
public class Ragdoll : MonoBehaviour {
  
Component[] bones;
Animator anim;

	void Start ()
	{
		bones = gameObject.GetComponentsInChildren<Rigidbody> (); 
		anim = GetComponent<Animator> ();
	}

	public void controlRagdoll ( bool enableRagdoll ) 
	{
		Debug.Log("controlRagdoll-number of bones: " + bones.Length );
		foreach (Rigidbody ragdoll in bones)
		{
			ragdoll.isKinematic = !enableRagdoll;

		}
	
		anim.enabled = !enableRagdoll;
	}
		
}
