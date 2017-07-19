using System.Collections;
using UnityEngine;

public enum RainType
{
	No_Rain = 0,
	Light_Rain = 1,
	Heavy_Rain =2
}

public class RainActivator : MonoBehaviour {

	void Start ()
	{
		ParticleSystem.EmissionModule module = GetComponent<ParticleSystem>().emission;
	    switch (LevelManager.Instance.rainType)
		{
	        case RainType.No_Rain:
				gameObject.SetActive( false );	
				break;
	                
	        case RainType.Light_Rain:
				module.rateOverTime = 100;
				break;

	        case RainType.Heavy_Rain:
				module.rateOverTime = 300;
				break;
		}
	}
	
}
