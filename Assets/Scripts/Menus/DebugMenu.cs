using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.GameCenter;

public class DebugMenu : MonoBehaviour {


	[Header("Debug Menu")]
	public Text titleText;
	[Header("Player Stats")]
	public Text currentStars;
	public Text lifetimeStars;
	public Text ownsStarDoubler;

	// Use this for initialization
	void Start () {
	
		#if UNITY_EDITOR
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu
		#endif

		//Text Inititialization
		titleText.text = LocalizationManager.Instance.getText("POPUP_TITLE_DEBUG");

		updatePlayerStats();
	}

	public void resetSavedData()
	{
		Debug.Log("resetSavedData");
		SoundManager.playButtonClick();
		PlayerStatsManager.Instance.resetPlayerStats();
	}

	public void deleteRequests()
	{
		Debug.Log("deleteRequests");
		SoundManager.playButtonClick();
		StartCoroutine( FacebookManager.Instance.deleteAllAppRequests() );
	}

	public void resetAchievements()
	{
		Debug.Log("resetAchievements");
		SoundManager.playButtonClick();
		GameCenterPlatform.ResetAllAchievements( (resetResult) => {
			Debug.Log( (resetResult) ? "Achievement Reset succesfull." : "Achievement Reset failed." );
		});
		GameCenterManager.resetAchievementsCompleted();
	}

	public void giveStars()
	{
		Debug.Log("Give 25000 Stars");
		SoundManager.playButtonClick();
		PlayerStatsManager.Instance.modifyCurrentCoins( 25000, false, false );
		PlayerStatsManager.Instance.savePlayerStats();
		updatePlayerStats();
	}

	public void giveLives()
	{
		Debug.Log("Give 20 Lives");
		SoundManager.playButtonClick();
		PlayerStatsManager.Instance.increaseLives( 20 );
		PlayerStatsManager.Instance.savePlayerStats();
	}

	public void giveTreasureChestKeys()
	{
		Debug.Log("Give 25 Treasure Chest Keys");
		SoundManager.playButtonClick();
		PlayerStatsManager.Instance.increaseTreasureKeysOwned( 25 );
		PlayerStatsManager.Instance.savePlayerStats();
	}

	void updatePlayerStats()
	{
		currentStars.text = "Current Stars: " + PlayerStatsManager.Instance.getCurrentCoins();
		lifetimeStars.text = "Lifetime Stars: " + PlayerStatsManager.Instance.getLifetimeCoins();
		if( PlayerStatsManager.Instance.getOwnsStarDoubler() )
		{
			ownsStarDoubler.text = "Owns Star Doubler: true";
		}
		else
		{
			ownsStarDoubler.text = "Owns Star Doubler: false";
		}
	}

}
