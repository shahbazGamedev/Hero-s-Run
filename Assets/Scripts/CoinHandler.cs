using UnityEngine;
using System.Collections;

public class CoinHandler : MonoBehaviour {

	public float chanceDisplayed = 0.3f;
	public int totalValueOfCoins = 0;
	
	void OnEnable ()
	{

		float chanceAvailable = chanceDisplayed * GameManager.Instance.getGlobalCoinMultiplier();
		if( Random.value <= chanceAvailable )
		{
			gameObject.SetActive( true );
			CoinManager.realNumberCoinsSpawned = CoinManager.realNumberCoinsSpawned + totalValueOfCoins;
		}
		else
		{
			gameObject.SetActive( false );
		}
		
	}
}
