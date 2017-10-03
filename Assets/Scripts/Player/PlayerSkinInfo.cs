using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkinInfo : MonoBehaviour {
	
		public UnityEngine.Avatar animatorAvatar;
		public RuntimeAnimatorController runtimeAnimatorController;

		[Tooltip("The transform parent that owns the children that have the rigid bodies used by the ragdoll system.")]
		public Transform ragdollRigidBodyParent;

}
