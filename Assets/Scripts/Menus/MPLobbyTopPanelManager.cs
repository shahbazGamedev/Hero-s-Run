using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MPLobbyTopPanelManager : MonoBehaviour {

	[Header("Top Panel")]
	public Text numberOfStarsText;
	public Image starDoublerIcon;

	// Use this for initialization
	void Start ()
	{
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
		//Debug.Log("MPLobbyTopPanelManager-PlayerInventoryChanged: " + eventType + " " + newValue );
		switch (eventType)
		{
			case PlayerInventoryEvent.Star_Changed:
				numberOfStarsText.text = newValue.ToString("N0");			
			break;

			case PlayerInventoryEvent.Star_Doubler_Changed:
				starDoublerIcon.gameObject.SetActive( PlayerStatsManager.Instance.getOwnsStarDoubler() );
			break;
		}
	}
}
