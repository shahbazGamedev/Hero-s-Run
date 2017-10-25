using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDTopPanelManager : MonoBehaviour {

	[Header("Top Panel Shared")]
	public GameObject contentPanel;
	public Image coinDoublerIcon;
	public Text numberOfLivesText;
	[Header("Top Panel - Story Mode")]
	public Image keysIcon;
	string numberOfChestKeysInEpisode;
	public Text numberOfKeysText;
	[Header("Top Panel - Endless Mode")]
	public Text distanceText;

	// Use this for initialization
	void Start ()
	{
		int episodeNumber = LevelManager.Instance.getCurrentEpisodeNumber();
		numberOfLivesText.text = PlayerStatsManager.Instance.getLives().ToString();
		coinDoublerIcon.gameObject.SetActive( PlayerStatsManager.Instance.getOwnsCoinDoubler() );
		if( GameManager.Instance.getGameMode() == GameMode.Story )
		{
			//Story mode					
			//Display the treasure key info but not the distance
			numberOfKeysText.gameObject.SetActive( true );
			keysIcon.gameObject.SetActive( true );
			distanceText.gameObject.SetActive( false );

			numberOfChestKeysInEpisode = "/" + LevelManager.Instance.getCurrentEpisodeInfo().numberOfChestKeys;
			numberOfKeysText.text = PlayerStatsManager.Instance.getNumberKeysFoundInEpisode(episodeNumber).ToString() + numberOfChestKeysInEpisode;
			distanceText.gameObject.SetActive( false );
		}
		else
		{
 			//Endless mode					
			//Display the distance but not the treasure key info
			numberOfKeysText.gameObject.SetActive( false );
			keysIcon.gameObject.SetActive( false );
			distanceText.gameObject.SetActive( true );
		}
		contentPanel.SetActive( false );
	}	

	void Update()
	{
		if( GameManager.Instance.getGameMode() == GameMode.Endless) distanceText.text = PlayerStatsManager.Instance.getDistanceTravelled().ToString() + " m";
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
 
			case PlayerInventoryEvent.Coin_Doubler_Changed:
				coinDoublerIcon.gameObject.SetActive( PlayerStatsManager.Instance.getOwnsCoinDoubler() );
			break;

			case PlayerInventoryEvent.Life_Changed:
				numberOfLivesText.text = newValue.ToString();	
			break;
		}
	}

	void GameStateChange( GameState previousState, GameState newState )
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
