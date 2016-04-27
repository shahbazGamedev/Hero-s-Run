using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDTopPanelManager : MonoBehaviour {

	[Header("HUD Top Panel")]
	public Text numberOfKeysText;
	public Text numberOfStarsText;

	// Use this for initialization
	void Start () {
	
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
		Debug.Log("HUDTopPanelManager-PlayerInventoryChanged: " + eventType + " " + newValue );
		if( eventType == PlayerInventoryEvent.Key_Changed )
		{
			numberOfKeysText.text = newValue.ToString();
		}
		else if( eventType == PlayerInventoryEvent.Star_Changed )
		{
			numberOfStarsText.text = newValue.ToString("N0");
		}
	}

}
