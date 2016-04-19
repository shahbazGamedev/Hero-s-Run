using UnityEngine;
using System.Collections;

public class TrapFireStreams : MonoBehaviour {

	public float duration = 3f;
	public float colliderMoveDuration = 1.8f;
	public float hiddenPosition = -2;
	public float endPosition = 2;

	public ParticleSystem leftFire;
	public ParticleSystem centerFire;
	public ParticleSystem rightFire;
	public GameObject leftCollider;
	public GameObject centerCollider;
	public GameObject rightCollider;

	ParticleSystem.EmissionModule em;

	enum Location {
		Left = 0,
		Center = 1,
		Right = 2
	}

	//We want 2 out 3 firestreams active at all times
	void activateFireStreams()
	{
		Location inactiveLocation = Utilities.GetRandomEnum<Location>();
		//print("inactiveLocation " + inactiveLocation );
		//Left fire
		if( inactiveLocation == Location.Left )
		{
			em = leftFire.GetComponent <ParticleSystem> ().emission;
			em.enabled = false;

			leftCollider.SetActive(false);
			leftCollider.transform.localPosition = new Vector3(leftCollider.transform.localPosition.x, hiddenPosition, leftCollider.transform.localPosition.z ); 

			if( !centerCollider.activeSelf )
			{
				em = centerFire.GetComponent <ParticleSystem> ().emission;
				em.enabled = true;
				centerCollider.SetActive(true);
				LeanTween.moveLocalY(centerCollider, endPosition, colliderMoveDuration);
				centerFire.GetComponent<AudioSource>().Play();

			}

			if( !rightCollider.activeSelf )
			{
				em = rightFire.GetComponent <ParticleSystem> ().emission;
				em.enabled = true;
				rightCollider.SetActive(true);
				LeanTween.moveLocalY(rightCollider, endPosition, colliderMoveDuration);
				rightFire.GetComponent<AudioSource>().Play();
			}
		}
		//Center fire
		else if( inactiveLocation == Location.Center )
		{
			if( !leftCollider.activeSelf )
			{
				em = leftFire.GetComponent <ParticleSystem> ().emission;
				em.enabled = true;
				leftCollider.SetActive(true);
				LeanTween.moveLocalY(leftCollider, endPosition, colliderMoveDuration);
				leftFire.GetComponent<AudioSource>().Play();
			}
			
			em = centerFire.GetComponent <ParticleSystem> ().emission;
			em.enabled = false;
			centerCollider.SetActive(false);
			centerCollider.transform.localPosition = new Vector3(centerCollider.transform.localPosition.x, hiddenPosition, centerCollider.transform.localPosition.z ); 

			if( !rightCollider.activeSelf )
			{
				em = rightFire.GetComponent <ParticleSystem> ().emission;
				em.enabled = true;
				rightCollider.SetActive(true);
				LeanTween.moveLocalY(rightCollider, endPosition, colliderMoveDuration);
				rightFire.GetComponent<AudioSource>().Play();
			}
		}
		//Right fire
		else
		{
			if( !leftCollider.activeSelf )
			{
				em = leftFire.GetComponent <ParticleSystem> ().emission;
				em.enabled = true;
				leftCollider.SetActive(true);
				LeanTween.moveLocalY(leftCollider, endPosition, colliderMoveDuration);
				leftFire.GetComponent<AudioSource>().Play();
			}
			
			if( !centerCollider.activeSelf )
			{
				em = centerFire.GetComponent <ParticleSystem> ().emission;
				em.enabled = true;
				centerCollider.SetActive(true);
				LeanTween.moveLocalY(centerCollider, endPosition, colliderMoveDuration);
				centerFire.GetComponent<AudioSource>().Play();
			}
			
			rightCollider.transform.localPosition = new Vector3(rightCollider.transform.localPosition.x, hiddenPosition, rightCollider.transform.localPosition.z ); 
			em = rightFire.GetComponent <ParticleSystem> ().emission;
			em.enabled = false;
			rightCollider.SetActive(false);
		}
	}
	
	void OnEnable()
	{
		InvokeRepeating( "activateFireStreams", 0, duration );
		LeanTween.resume(gameObject);
	}
	
	void OnDisable()
	{
		CancelInvoke("activateFireStreams");
		LeanTween.pause(gameObject);
	}
}
