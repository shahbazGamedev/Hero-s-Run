using UnityEngine;
using System.Collections;

public class CoinAttractor : BaseClass {

	private Transform mainCamera;

	//To move coins to the Coin icon at the top right of the screen
	Vector3 coinScreenPos;
	Vector3 coinDestination = new Vector3( 0,0,0 );

	void Awake ()
	{
		mainCamera = Camera.main.transform;
	}

	// Use this for initialization
	void Start ()
	{
		//For coins
		Vector2 coinIconPos = HUDHandler.hudHandler.getCoinIconPos();
		coinScreenPos = new Vector3( coinIconPos.x, coinIconPos.y, 3f );
	}
		
	IEnumerator attractCoin( Transform coin )
	{
		//First, attract the coin to the center of the character
   		Vector3 originalPos = coin.position;
		//Time in seconds to reach the coin counter
		float time = 0.7f;
   		float originalTime = time;
   		coin.localScale = new Vector3( 0.12f, 0.12f, 0.04f );
 
		while ( time > 0.0f )
		{
			time -= Time.deltaTime;
			coinDestination.Set ( transform.position.x, transform.position.y + 0.9f, transform.position.z );

			coin.position = Vector3.Lerp( coinDestination, originalPos, time / originalTime );
			
			yield return _sync();
		}
		
		//Second, move the coin to the coin counter at the top of the screen
		//We don't want it to turn while flying up
		Destroy ( coin.gameObject.GetComponent ( "Rotator" ) );
		originalPos = coin.position;
		//Time in seconds to reach the coin counter
		time = 0.75f;
   		originalTime = time;
   		coin.localScale = new Vector3( 0.06f, 0.06f, 0.06f );
		Behaviour halo = (Behaviour)coin.gameObject.GetComponent("Halo");
		if( halo != null ) halo.enabled = false;
 
		while ( time > 0.0f )
		{
			time -= Time.deltaTime;
			Vector3 coinDestination = mainCamera.GetComponent<Camera>().ScreenToWorldPoint (coinScreenPos);

			if( coin != null )
			{
				coin.position = Vector3.Lerp( coinDestination, originalPos, time / originalTime );
				coin.rotation = mainCamera.rotation;

			}
			
			yield return _sync();
		}
		if( coin != null )
		{
			Destroy ( coin.gameObject );
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if ( PowerUpManager.isThisPowerUpActive( PowerUpType.Magnet ) && other.name.StartsWith( "Coin" ) )
		{
			//Disable the collider
			other.enabled = false;
			PlayerStatsManager.Instance.modifyCoinCount( other.gameObject );
			CoinManager.playCoinPickupSound();
			StartCoroutine( attractCoin( other.transform ) );
		}
	}
	
	//We also want to attract all the coins that are already within the collision sphere
	public void attractCoinsWithinSphere(Vector3 center, float radius)
	{
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        int i = 0;
        while (i < hitColliders.Length)
		{
			if( hitColliders[i].name.StartsWith( "Coin" ) )
			{
				PlayerStatsManager.Instance.modifyCoinCount( hitColliders[i].gameObject );
				CoinManager.playCoinPickupSound();
				StartCoroutine( attractCoin( hitColliders[i].transform ) );
			}
            i++;
        }
    }

}
