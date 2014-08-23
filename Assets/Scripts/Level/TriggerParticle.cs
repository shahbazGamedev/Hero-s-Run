using UnityEngine;
using System.Collections;

public class TriggerParticle : MonoBehaviour {

	public ParticleSystem effect;

	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" )
		{
			//Send an event to interested classes
			if(effect != null) effect.Play();
		}
	}

}
