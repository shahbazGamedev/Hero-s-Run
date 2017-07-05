using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//For SetLookAtPosition to work, there are 2 conditions:
//The rig must be Humanoid
//In the Animator windows, under Layers, under Settings, you must have the IK Pass toggled on.
public class PlayerIK : MonoBehaviour {

	[Header("Look At IK")]
	public Transform lookAtTarget;
	Animator anim;
	[SerializeField] float lookAtWeight = 0.8f;
	[SerializeField] float bodyWeight = 0.7f;
	[SerializeField] float headWeight = 1f;
	[SerializeField] float eyesWeight = 1f;
	[SerializeField] float clampWeight = 1f;
	[SerializeField] float activeDistanceIK = 24f;
	[SerializeField] float dotProductIK = 0.55f;
	bool lookAtActive = false;
	bool enableIK = true;

	protected void Awake ()
	{
		anim = GetComponent<Animator>();
	}

	/*
		returns:
		-1 if creature is behind player
		+1 if creature is in front
		0 if creature is on the side
		0.5 if creature is facing player and within 60 degrees (i.e. between 30 degrees to the left and 30 degrees to the right)
	*/
	float getDotProduct()
	{
		Vector3 heading = lookAtTarget.position - transform.position;
		return Vector3.Dot( heading.normalized, transform.forward );
	}

	void OnAnimatorIK()
	{
		if( lookAtTarget != null && enableIK && getDotProduct() > dotProductIK )
		{
			float distance = Vector3.Distance(lookAtTarget.position,transform.position);
			if( distance < activeDistanceIK )			
			{
				if( !lookAtActive )
				{
 					StartCoroutine( fadeInLookAtPosition( 0.8f, 0.7f ) );
				} 
				anim.SetLookAtPosition( lookAtTarget.position );
				anim.SetLookAtWeight( lookAtWeight, bodyWeight, headWeight, eyesWeight, clampWeight );
			}
		}
	}

	public IEnumerator fadeOutLookAtPosition( float finalWeight, float stayDuration, float fadeDuration )
	{
		float elapsedTime = 0;
		
		//Stay
		yield return new WaitForSeconds(stayDuration);
		
		//Fade out
		elapsedTime = 0;
		
		float initialWeight = lookAtWeight;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			lookAtWeight = Mathf.Lerp( initialWeight, finalWeight, elapsedTime/fadeDuration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < fadeDuration );
		
		lookAtWeight = finalWeight;
	
	}

	public IEnumerator fadeInLookAtPosition( float finalWeight, float fadeDuration )
	{
		lookAtActive = true;
		float elapsedTime = 0;

		//Fade in
		elapsedTime = 0;
		
		float initialWeight = 0;
		
		do
		{
			elapsedTime = elapsedTime + Time.deltaTime;
			lookAtWeight = Mathf.Lerp( initialWeight, finalWeight, elapsedTime/fadeDuration );
			yield return new WaitForFixedUpdate();  
			
		} while ( elapsedTime < fadeDuration );
		
		lookAtWeight = finalWeight;
	
	}

	public void playerDied()
	{
		if( lookAtTarget != null && enableIK ) StartCoroutine( fadeOutLookAtPosition( 0.2f, 0, 0.9f ) );
	}

}
