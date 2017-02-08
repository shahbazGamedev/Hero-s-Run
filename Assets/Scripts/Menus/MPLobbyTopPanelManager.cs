using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MPLobbyTopPanelManager : MonoBehaviour {

	[Header("Top Panel")]
	public Text numberOfCoinsText;
	public Image coinDoublerIcon;

	// Use this for initialization
	void Start ()
	{
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
		//Debug.Log("MPLobbyTopPanelManager-PlayerInventoryChanged: " + eventType + " " + newValue );
		switch (eventType)
		{
			case PlayerInventoryEvent.Coin_Changed:
				numberOfCoinsText.text = newValue.ToString("N0");			
			break;

			case PlayerInventoryEvent.Coin_Doubler_Changed:
				coinDoublerIcon.gameObject.SetActive( PlayerStatsManager.Instance.getOwnsCoinDoubler() );
			break;
		}
	}

	public void OnClickShowStore()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		StoreManager.Instance.showStore( StoreTab.Store, StoreReason.None );
	}

}
