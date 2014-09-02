using UnityEngine;
using System.Collections;

public class StumbleHandler : MonoBehaviour {


	public float chanceDisplayed = 0.2f;

	void Start ()
	{

		float chanceAvailable = chanceDisplayed * GameManager.Instance.getGlobalStumbleMultiplier();
		if( Random.value <= chanceAvailable )
		{
			gameObject.SetActive( true );
		}
		else
		{
			gameObject.SetActive( false );
		}

	}

}
