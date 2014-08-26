using UnityEngine;
using System.Collections;

public class DragonCoinManager : MonoBehaviour {
	
	PlayerController playerController;
	
	// Use this for initialization
	void Start () {
	
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		playerController = (PlayerController) player.GetComponent(typeof(PlayerController));
	}
	
	void OnTriggerEnter(Collider other)
	{
		if ( other.name.StartsWith( "Coin" )  )
		{
			//Disable the collider
			other.enabled = false;
			PlayerStatsManager.Instance.modifyCoinCount( other.gameObject );
			CoinManager.playCoinPickupSound();
			//We don't want it to turn while flying up
			Destroy ( other.GetComponent ( "Rotator" ) );
			//Get the player controller to handle the coin
			StartCoroutine( playerController.moveCoin( other.transform ) );
		}
	}
}
