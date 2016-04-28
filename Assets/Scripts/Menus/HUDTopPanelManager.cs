using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDTopPanelManager : MonoBehaviour {

	[Header("HUD Top Panel")]
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
	}	

	void OnEnable()
	{
		PlayerStatsManager.playerInventoryChanged += PlayerInventoryChanged;
	}
	
	void OnDisable()
	{
		PlayerStatsManager.playerInventoryChanged -= PlayerInventoryChanged;
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

}
