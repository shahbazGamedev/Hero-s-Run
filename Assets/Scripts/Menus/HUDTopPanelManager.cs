using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDTopPanelManager : MonoBehaviour {

	[Header("Top Panel")]
	public GameObject contentPanel;
	public Text episodeNumberText;
	string numberOfChestKeysInEpisode;
	public Text numberOfKeysText;
	public Text numberOfLivesText;
	public Image starDoublerIcon;

	// Use this for initialization
	void Start ()
	{
		int episodeNumber = LevelManager.Instance.getCurrentEpisodeNumber();
		episodeNumberText.text = "~ " + (episodeNumber + 1 ).ToString() + " ~";
		numberOfChestKeysInEpisode = "/" + LevelManager.Instance.getCurrentEpisodeInfo().numberOfChestKeys;
		numberOfKeysText.text = PlayerStatsManager.Instance.getNumberKeysFoundInEpisode(episodeNumber).ToString() + numberOfChestKeysInEpisode;
		numberOfLivesText.text = PlayerStatsManager.Instance.getLives().ToString();
		starDoublerIcon.gameObject.SetActive( PlayerStatsManager.Instance.getOwnsStarDoubler() );
		contentPanel.SetActive( false );
	}	

	void OnEnable()
	{
		PlayerStatsManager.playerInventoryChanged += PlayerInventoryChanged;
		GameManager.gameStateEvent += GameStateChange;
	}
	
	void OnDisable()
	{
		PlayerStatsManager.playerInventoryChanged -= PlayerInventoryChanged;
		GameManager.gameStateEvent -= GameStateChange;
	}

	void PlayerInventoryChanged( PlayerInventoryEvent eventType, int newValue )
	{
		//Debug.Log("HUDTopPanelManager-PlayerInventoryChanged: " + eventType + " " + newValue );
		switch (eventType)
		{
			case PlayerInventoryEvent.Key_Found_In_Episode_Changed:
				numberOfKeysText.text = newValue.ToString() + numberOfChestKeysInEpisode;
			break;
 
			case PlayerInventoryEvent.Star_Doubler_Changed:
				starDoublerIcon.gameObject.SetActive( PlayerStatsManager.Instance.getOwnsStarDoubler() );
			break;

			case PlayerInventoryEvent.Life_Changed:
				numberOfLivesText.text = newValue.ToString();	
			break;
		}
	}

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Normal )
		{
			if( contentPanel != null ) contentPanel.SetActive( true );
		}
		else
		{
			if( contentPanel != null ) contentPanel.SetActive( false );
		}
	}

}
