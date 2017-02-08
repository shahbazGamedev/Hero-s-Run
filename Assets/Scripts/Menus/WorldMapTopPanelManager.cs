using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WorldMapTopPanelManager : MonoBehaviour {

	[Header("Top Panel")]
	public Text numberOfKeysText;
	public Text numberOfLivesText;
	public Text numberOfCoinsText;
	public Image coinDoublerIcon;

	// Use this for initialization
	void Start ()
	{
		if( numberOfKeysText != null ) numberOfKeysText.text = PlayerStatsManager.Instance.getTreasureKeysOwned().ToString();
		numberOfLivesText.text = PlayerStatsManager.Instance.getLives().ToString();
		numberOfCoinsText.text = PlayerStatsManager.Instance.getCurrentCoins().ToString("N0");
		coinDoublerIcon.gameObject.SetActive( PlayerStatsManager.Instance.getOwnsCoinDoubler() );
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

			case PlayerInventoryEvent.Coin_Changed:
				numberOfCoinsText.text = newValue.ToString("N0");			
			break;

			case PlayerInventoryEvent.Coin_Doubler_Changed:
				coinDoublerIcon.gameObject.SetActive( PlayerStatsManager.Instance.getOwnsCoinDoubler() );
			break;
		}
	}

}
