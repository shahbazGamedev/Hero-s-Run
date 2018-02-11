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
	[SerializeField] GameObject challengeReward;
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

	#region Okay button
	public void OnClickOkay()
	{
		StartCoroutine( HUDMultiplayer.hudMultiplayer.returnToMatchmakingAfterDelay( 0 ) );
	}
	#endregion

	#region Reward Boxes
	public void displayLootBox()
	{
		lootBoxReward.SetActive( true );
	}

	public void displaySoftCurrency( int softCurrencyGranted )
	{
		softCurrencyReward.SetActive( true );
		softCurrencyAmount.text = softCurrencyGranted.ToString();
	}

	public void displayXP()
	{
		xpReward.SetActive( true );
		StartCoroutine( displayIndividualAwards() );
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

	public void displayChallenge()
	{
		challengeReward.SetActive( true );
	}
	#endregion

}
