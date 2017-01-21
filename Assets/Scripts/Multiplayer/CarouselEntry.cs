using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarouselEntry : MonoBehaviour {

	[Header("Race Circuit Parameters")]
	public int circuitNumber = 0; 	//This value corresponds to the index in the multiplayerList of LevelData
	public Text circuitName;
	public Image circuitImage;
	public Text entryFee;

	[Header("Online Players")]
	public Text numberOnlinePlayers;

	[Header("Shared")]
	public Text raceButtonText;
	public Text exitButtonText;
	public Text prizesLabelText;

	[Header("Prizes")]
	//Prize 1
	public Image prize1Image;
	public Text prize1;

	//Prize 2
	public Image prize2Image;
	public Text prize2;

	//Prize 3
	public Image prize3Image;
	public Text prize3;
	
	// Use this for initialization
	void Awake ()
	{
		//Configuration
		LevelData levelData = LevelManager.Instance.getLevelData();
		LevelData.CircuitInfo circuitInfo = levelData.getMultiplayerInfo( circuitNumber ).circuitInfo;

		//Circuit
		circuitName.text = LocalizationManager.Instance.getText( circuitInfo.circuitTextID );
		circuitImage.sprite = circuitInfo.circuitSprite;

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

		//Hack - not implemented
		//Number of online players in that circuit
		numberOnlinePlayers.text = Random.Range( 1000, 50001 ).ToString();

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
