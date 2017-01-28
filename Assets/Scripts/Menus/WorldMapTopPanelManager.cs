using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WorldMapTopPanelManager : MonoBehaviour {

	[Header("Top Panel")]
	public Text numberOfKeysText;
	public Text numberOfLivesText;
	public Text numberOfStarsText;
	public Image starDoublerIcon;

	// Use this for initialization
	void Start ()
	{
		if( numberOfKeysText != null ) numberOfKeysText.text = PlayerStatsManager.Instance.getTreasureKeysOwned().ToString();
		numberOfLivesText.text = PlayerStatsManager.Instance.getLives().ToString();
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
		//Debug.Log("WorldMapTopPanelManager-PlayerInventoryChanged: " + eventType + " " + newValue );
		switch (eventType)
		{
			case PlayerInventoryEvent.Key_Changed:
				if( numberOfKeysText != null ) numberOfKeysText.text = PlayerStatsManager.Instance.getTreasureKeysOwned().ToString();
			break;
 
			case PlayerInventoryEvent.Life_Changed:
				if( numberOfLivesText != null ) numberOfLivesText.text = newValue.ToString();	
			break;

			case PlayerInventoryEvent.Star_Changed:
				numberOfStarsText.text = newValue.ToString("N0");			
			break;

			case PlayerInventoryEvent.Star_Doubler_Changed:
				starDoublerIcon.gameObject.SetActive( PlayerStatsManager.Instance.getOwnsStarDoubler() );
			break;
		}
	}

}
