using System.Collections;
using UnityEngine;
using UnityEngine.Apple.ReplayKit;
using System;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;


public class ResultsHandler : MonoBehaviour {

	[SerializeField] GameObject lootBoxReward;
	[SerializeField] GameObject softCurrencyReward;
	[SerializeField] TextMeshProUGUI softCurrencyAmount;
	[SerializeField] GameObject xpReward;
	[SerializeField] TextMeshProUGUI reasonAwardedXP;
	[SerializeField] TextMeshProUGUI totalXPAwarded;
	[SerializeField] GameObject stayAsTeamReward;
	public Button okayButton;
	public List<GameObject> emotesList = new List<GameObject>();

	const float SPIN_DURATION = 2f;
	const float DELAY_BETWEEN_XP_AWARDS = 3.75f;

	protected PlayerRace getOtherPlayer( PlayerRace localPlayerRace )
	{
		for(int i=0; i<PlayerRace.players.Count;i++)
		{
			if( PlayerRace.players[i] != localPlayerRace ) return PlayerRace.players[i];
		}
		return null;
	}

	public GameObject getEmoteGameObjectForPlayerNamed( string playerName )
	{
		GameObject emote = emotesList.Find( go => go.name == playerName);
		if ( emote == null ) Debug.LogError("ResultsHandler-could not find emote game object for player " + playerName );
		return emote;
	}

	#region Save rewards
	protected void saveRewards()
	{
		Debug.Log( "ResultsHandler-saving rewards." );
		//Soft currency and loot box
		GameManager.Instance.playerInventory.serializePlayerInventory( false );
		//Xp
		GameManager.Instance.playerProfile.serializePlayerprofile( false );

		//Now save
		PlayerStatsManager.Instance.savePlayerStats();		
	}
	#endregion

	#region Okay button
	public void OnClickOkay()
	{
		StartCoroutine( HUDMultiplayer.hudMultiplayer.returnToMatchmakingAfterDelay( 0 ) );
	}
	#endregion

	#region Reward Boxes
	protected void displayLootBox()
	{
		lootBoxReward.SetActive( true );
		GameManager.Instance.playerInventory.addLootBox( new LootBoxOwnedData( LootBoxType.RACE_WON, GameManager.Instance.playerProfile.getCurrentSector(), LootBoxState.READY_TO_UNLOCK ) );
		Debug.Log( "ResultsHandler-displayLootBox: addLootBox: " + LootBoxType.RACE_WON );
	}

	protected void displaySoftCurrency( int softCurrencyGranted )
	{
		softCurrencyReward.SetActive( true );
		softCurrencyAmount.text = softCurrencyGranted.ToString();
		GameManager.Instance.playerInventory.addCoins( softCurrencyGranted );
		Debug.Log( "ResultsHandler-displaySoftCurrency: addCoins: " + softCurrencyGranted );
	}

	protected void displayXP()
	{
		xpReward.SetActive( true );
		int xpAwarded = calculateTotalXPEarned();
		GameManager.Instance.playerProfile.addToTotalXPEarned( xpAwarded, false );
		Debug.Log( "ResultsHandler-displayXP: addToTotalXPEarned: " + xpAwarded );
		StartCoroutine( displayIndividualAwards() );
	}

	int calculateTotalXPEarned()
	{
		XPAwardType awardType;
		int xpAwarded = 0;
		ProgressionManager.XPAward xpAward;
		for( int i = 0; i < PlayerRaceManager.Instance.raceAwardList.Count; i++ )
		{
			awardType = PlayerRaceManager.Instance.raceAwardList[i];
			xpAward = ProgressionManager.Instance.getXPAward( awardType );
			xpAwarded = xpAwarded + xpAward.xpAmount;
		}
		return xpAwarded;
	}

	IEnumerator displayIndividualAwards()
	{
		yield return new WaitForEndOfFrame();
		//Example: "CONSECUTIVE MATCH<color=orange>+200xp</color>"
		XPAwardType awardType;
		int xpAwarded = 0;
		string xpAwardedLocalized = LocalizationManager.Instance.getText( "RESULTS_XP_TOTAL_AWARDED" );
		ProgressionManager.XPAward xpAward;
		for( int i = 0; i < PlayerRaceManager.Instance.raceAwardList.Count; i++ )
		{
			awardType = PlayerRaceManager.Instance.raceAwardList[i];
			xpAward = ProgressionManager.Instance.getXPAward( awardType );

			//Don't display an XP award where the XP amount is 0.
			if( xpAward.xpAmount == 0 ) continue;

			string awardText = LocalizationManager.Instance.getText( "XP_AWARD_" + awardType.ToString() );
			reasonAwardedXP.text = awardText + "<color=orange>+" + xpAward.xpAmount.ToString() + "xp</color>";

			//Update the xpAwarded
			totalXPAwarded.GetComponent<UISpinNumber>().spinNumber( "{0}", xpAwarded, xpAwarded + xpAward.xpAmount, SPIN_DURATION, true );
			xpAwarded = xpAwarded + xpAward.xpAmount;
			yield return new WaitForSecondsRealtime( DELAY_BETWEEN_XP_AWARDS );
		}
		//Remove the last award text after a moment to make the XP total stand out more.
		reasonAwardedXP.gameObject.SetActive( false );
	}

	protected void displayStayAsTeam()
	{
		stayAsTeamReward.SetActive( true );
	}
	#endregion

}
