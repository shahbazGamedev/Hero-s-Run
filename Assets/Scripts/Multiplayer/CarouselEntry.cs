using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

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
	[SerializeField] Text prizesLabelText;

	[Header("Prizes")]
	//Prize 1
	[SerializeField] Image prize1Image;
	[SerializeField] Text prize1;

	//Prize 2
	[SerializeField] Image prize2Image;
	[SerializeField] Text prize2;

	//Prize 3
	[SerializeField] Image prize3Image;
	[SerializeField] Text prize3;
	
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
		raceButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_RACE" );
		exitButtonText.text = LocalizationManager.Instance.getText( "CIRCUIT_EXIT" );
		prizesLabelText.text = LocalizationManager.Instance.getText( "CIRCUIT_PRIZES" );

		//Prizes
		for( int i = 0; i < circuitInfo.prizeInfoList.Count; i++ )
		{
			configurePrize( i, circuitInfo.prizeInfoList[i] );
		}
	}

	void Start ()
	{
		LevelData levelData = LevelManager.Instance.getLevelData();
		LevelData.CircuitInfo circuitInfo = levelData.getMultiplayerInfo( circuitNumber ).circuitInfo;

		//Number of online players for this circuit for all Elo ratings.
		//We call this here because we want MPNetworkLobbyManager to be available.
		getNumberOfOnlinePlayers( circuitInfo.matchName );
	}
	
	void getNumberOfOnlinePlayers( string matchName )
	{
		//Player is connected to the Internet
		if( Application.internetReachability != NetworkReachability.NotReachable )
		{
			MPNetworkLobbyManager.Instance.StartMatchMaker();
			MPNetworkLobbyManager.Instance.matchMaker.ListMatches( 0, 100, matchName , false, 0, 0, OnMatchListOnlinePlayerCount );
		}
		else
		{
			numberOnlinePlayers.text = LocalizationManager.Instance.getText( "CIRCUIT_NOT_AVAILABLE" );
		}
	}

	void OnMatchListOnlinePlayerCount(bool success, string extendedInfo, List<MatchInfoSnapshot> matchInfoSnapshotList )
	{
		if( success )
		{	
			int onlinePlayerCount = 0;
			for( int i = 0; i < matchInfoSnapshotList.Count; i++ )
			{
				onlinePlayerCount = onlinePlayerCount + matchInfoSnapshotList[i].currentSize;
				Debug.Log("MPNetworkLobbyManager-OnMatchListOnlinePlayerCount: " + i + " Name: " + matchInfoSnapshotList[i].name + " Max size " + matchInfoSnapshotList[i].maxSize + " Current size " + matchInfoSnapshotList[i].currentSize );
			}
			//If OnMatchListOnlinePlayerCount gets called after we have left the Circuit Selection scene, the text field, numberOnlinePlayers, will be null. This is why we do a null check.
			if( numberOnlinePlayers != null ) numberOnlinePlayers.text = onlinePlayerCount.ToString();
		}
		else
		{
			Debug.LogWarning("CarouselEntry-OnMatchListOnlinePlayerCount: " + extendedInfo );
			if( numberOnlinePlayers != null ) numberOnlinePlayers.text = LocalizationManager.Instance.getText( "CIRCUIT_NOT_AVAILABLE" );
		}
	}

	void configurePrize( int index, LevelData.PrizeInfo prizeInfo )
	{
    	switch (index)
		{
	        case 0:
				prize1Image.sprite = prizeInfo.prizeIcon;
				prize1.text = configurePrizeText( prizeInfo );
                break;
	                
	        case 1:
				prize2Image.sprite = prizeInfo.prizeIcon;
				prize2.text = configurePrizeText( prizeInfo );
                break;
	                
	        case 2:
 				prize3Image.sprite = prizeInfo.prizeIcon;
				prize3.text = configurePrizeText( prizeInfo );
                break;      
		}
	}

	string configurePrizeText( LevelData.PrizeInfo prizeInfo )
	{
		string prizeText = string.Empty;
		string prizeTextString;
    	switch (prizeInfo.rewardType)
		{
	        case RewardType.Coins:
				prizeTextString = LocalizationManager.Instance.getText( "CIRCUIT_PRIZE" );
				prizeTextString = prizeTextString.Replace("<prize quantity>", prizeInfo.quantity.ToString() );
				prizeText = prizeTextString.Replace("<prize type>", LocalizationManager.Instance.getText( "CIRCUIT_COINS" ) );
                break;
	                
	        case RewardType.PowerUp:
				string powerUpWord = string.Empty;
				if( prizeInfo.quantity == 1 )
				{
					powerUpWord = LocalizationManager.Instance.getText( "CIRCUIT_POWER_UP_SINGULAR" );
				}
				else
				{
					powerUpWord = LocalizationManager.Instance.getText( "CIRCUIT_POWER_UP_PLURAL" );
				}
				prizeTextString = LocalizationManager.Instance.getText( "CIRCUIT_PRIZE" );
				prizeTextString = prizeTextString.Replace("<prize quantity>", prizeInfo.quantity.ToString() );
				prizeText = prizeTextString.Replace("<prize type>", powerUpWord );
                break;
	                
	        case RewardType.Customization:
 				prizeText = LocalizationManager.Instance.getText( prizeInfo.customizationPrizeTextID );
                break;      
		}
		return prizeText;
	}

}
