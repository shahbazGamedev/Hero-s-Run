using System.Collections;
using UnityEngine;

public class ZombieHand : MonoBehaviour {

	[SerializeField] float startDelay = 2f;
	[SerializeField] ParticleSystem spurt;
	[SerializeField] Rigidbody looseWoodPlank;
	[SerializeField] Vector3 woodPlankForce;
	[SerializeField] Vector3 woodPlankTorque;

	void Start ()
	{
		StartCoroutine( zombieHandPierceUp() );
	}
	
	IEnumerator zombieHandPierceUp()
	{
		yield return new WaitForSeconds( startDelay );

		spurt.Play();

		LeanTween.moveLocalY(gameObject, transform.position.y + 1.1f, 0.5f ).setEase(LeanTweenType.easeOutExpo).setOnComplete(zombieHandAnimation).setOnCompleteParam(gameObject);
		GetComponent<AudioSource>().PlayDelayed(0.1f);
		if( looseWoodPlank != null )
		{
			looseWoodPlank.AddForce( woodPlankForce );	
			looseWoodPlank.AddTorque( woodPlankTorque );
		}
	}
	
	void zombieHandAnimation()
	{
		GetComponent<Animation>().Play("FistToSearch" );
		GetComponent<Animation>().PlayQueued("Search", QueueMode.CompleteOthers );
	}
}
