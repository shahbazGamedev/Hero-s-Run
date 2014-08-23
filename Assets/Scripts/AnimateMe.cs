using UnityEngine;
using System.Collections;

public class AnimateMe : MonoBehaviour {
	
	public AnimationClip animationClip;
	[Range(0, 1f)] public float animationSpeed;
	
	// Use this for initialization
	void OnEnable ()
	{
		Animation anim = (Animation) transform.GetComponent("Animation");
		if( anim != null )
		{
			anim.wrapMode = WrapMode.Loop;
			anim[animationClip.name].speed = animationSpeed;
			anim.Play(animationClip.name);
		}
		else
		{
			Debug.LogWarning( "The GameObjet " + transform.name + " does not have an animation component.");
		}
	}
	
}
