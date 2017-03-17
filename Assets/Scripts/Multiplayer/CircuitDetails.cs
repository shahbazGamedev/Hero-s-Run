using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircuitDetails : MonoBehaviour {

	[Header("Race Circuit Parameters")]
	public  Text circuitName;
	public  Image circuitImage;

	[Header("Online Players")]
	[SerializeField] Text numberOnlinePlayers;

	// Use this for initialization
	void Awake ()
	{
		//Configuration
		LevelData.CircuitInfo circuitInfo = LevelManager.Instance.getSelectedCircuitInfo();

		//Circuit
		circuitName.text = LocalizationManager.Instance.getText( circuitInfo.circuitTextID );
		circuitImage.sprite = circuitInfo.circuitImage;

	}

	void Start ()
	{
		Invoke("getNumberOfOnlinePlayers", 2f );
	}
	
	void getNumberOfOnlinePlayers()
	{
		//The player needs to be connected to the Internet, connected to Photon and not be in offline mode.
		//Note: if PhotonNetwork.offlineMode is true, PhotonNetwork.connected will also be true, hence the extra test.
		if( Application.internetReachability != NetworkReachability.NotReachable && PhotonNetwork.connected && !PhotonNetwork.offlineMode )
		{
			//The count of players currently using this application (available on MasterServer in 5sec intervals).
			//This is the total for ALL tracks.
			numberOnlinePlayers.text = PhotonNetwork.countOfPlayers.ToString();

		}
		else
		{
			numberOnlinePlayers.text = LocalizationManager.Instance.getText( "CIRCUIT_NOT_AVAILABLE" );
		}
	}
}
