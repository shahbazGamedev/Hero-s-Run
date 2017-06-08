using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrownLootBoxHandler : MonoBehaviour {

	[SerializeField] TextMeshProUGUI crownsOwned;
	[SerializeField] Slider crownsOwnedProgressBar;
	public const int CROWNS_NEEDED_TO_OPEN = 10;

	// Use this for initialization
	void Start ()
	{
		string crownLootBoxOwned = LocalizationManager.Instance.getText( "CROWN_LOOT_BOX_OWNED" );
		crownsOwned.text = string.Format( crownLootBoxOwned, GameManager.Instance.playerInventory.getCrownBalance(), CROWNS_NEEDED_TO_OPEN );
		crownsOwnedProgressBar.value = GameManager.Instance.playerInventory.getCrownBalance()/(float)CROWNS_NEEDED_TO_OPEN;
	}
	
}
