using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDTopPanelManager : MonoBehaviour {

	[Header("HUD Top Panel")]
	public GameObject contentPanel;
	public Text numberOfKeysText;
	public Text numberOfLivesText;
	public Text numberOfStarsText;
	public Image starDoublerIcon;

	// Use this for initialization
	void Start ()
	{
		numberOfKeysText.text = PlayerStatsManager.Instance.getTreasureKeysOwned().ToString();
		if( numberOfLivesText != null ) numberOfLivesText.text = PlayerStatsManager.Instance.getLives().ToString();
		numberOfStarsText.text = PlayerStatsManager.Instance.getCurrentCoins().ToString("N0");
		starDoublerIcon.gameObject.SetActive( PlayerStatsManager.Instance.getOwnsStarDoubler() );
		if( contentPanel != null ) contentPanel.SetActive( false );
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
			case PlayerInventoryEvent.Key_Changed:
				numberOfKeysText.text = newValue.ToString();
			break;
		        
			case PlayerInventoryEvent.Star_Changed:
				numberOfStarsText.text = newValue.ToString("N0");			
			break;
		        
			case PlayerInventoryEvent.Star_Doubler_Changed:
				starDoublerIcon.gameObject.SetActive( PlayerStatsManager.Instance.getOwnsStarDoubler() );
			break;
		        
			case PlayerInventoryEvent.Life_Changed:
				if( numberOfLivesText != null ) numberOfLivesText.text = newValue.ToString();	
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
