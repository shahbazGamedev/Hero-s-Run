using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarouselEntry : MonoBehaviour {

	[Header("Race Circuit Parameters")]
	[SerializeField] int circuitNumber = 0; 	//This value corresponds to the index in the multiplayerList of LevelData
	public Text circuitName;
	public Image circuitImage;
	public Text entryFee;

	[Header("Online Players")]
	[SerializeField] Text numberOnlinePlayers;

	[Header("Shared")]
	[SerializeField] Text raceButtonText;
	[SerializeField] Text exitButtonText;
	
	// Use this for initialization
	void Awake ()
	{
		//Configuration
		LevelData levelData = LevelManager.Instance.getLevelData();
		LevelData.CircuitInfo circuitInfo = levelData.getMultiplayerInfo( circuitNumber ).circuitInfo;

		//Circuit
		circuitName.text = LocalizationManager.Instance.getText( circuitInfo.circuitTextID );
		circuitImage.sprite = circuitInfo.circuitImage;

		//Entry fee
		string entryFeeString = LocalizationManager.Instance.getText( "CIRCUIT_ENTRY_FEE" );
		if( circuitInfo.entryFee == 0 )
		{
			entryFeeString = entryFeeString.Replace("<entry fee>", LocalizationManager.Instance.getText( "MENU_FREE" ) );
		}
		else
		{
			entryFeeString = entryFeeString.Replace("<entry fee>", circuitInfo.entryFee.ToString() );
		}
		entryFee.text = entryFeeString;

		//Common to all carousel entries
		raceButtonText.text = LocalizationManager.Instance.getText( "HERO_SELECTION_CONFIRM" );
		exitButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_EXIT" );

	}

	void Start ()
	{
		InvokeRepeating("getNumberOfOnlinePlayers", 0f, 5f );
	}
	
	void getNumberOfOnlinePlayers()
	{
		//Player is connected to the Internet
		if( Application.internetReachability != NetworkReachability.NotReachable )
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
