using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StoreEntry : MonoBehaviour {

	[Header("Store Entry")]
	public PowerUpType powerUpType = PowerUpType.None;
	public Text title;
	public Text description;
	public Text buttonLabel;
	public Slider upgradeLevel;
	public string titleID = "POWER_UP_MAGNET";
	public string descriptionID = "POWER_UP_MAGNET_DESCRIPTION";


	// Use this for initialization
	void Awake () {
	
		LocalizationManager.Instance.initialize(); //For debugging, so I can see the text displayed without going through the load menu

		//We have 4 Boosts
		//Star Magnet
		title.text = LocalizationManager.Instance.getText(titleID);
		description.text = LocalizationManager.Instance.getText(descriptionID);
		buttonLabel.text = ( (PlayerStatsManager.Instance.getPowerUpUpgradeLevel( powerUpType ) + 1 )* 1000).ToString("N0");
		//Valid upgrade values are 0 to 6 included as Integer
		PlayerStatsManager.Instance.loadPlayerStats();
		upgradeLevel.value = PlayerStatsManager.Instance.getPowerUpUpgradeLevel( powerUpType ); 

	}
	
}
