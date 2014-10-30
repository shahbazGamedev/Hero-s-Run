using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour {

	[Header("Misc")]
	public Text playerCurrency;
	public Text upgradeTitle;
	public Text consumableTitle;
	bool levelLoading = false;
	public Canvas powerupCanvas;
	public Canvas storeCanvas;

	// Use this for initialization
	void Awake ()
	{
		if( playerCurrency != null ) playerCurrency.text = ( PlayerStatsManager.Instance.getLifetimeCoins() ).ToString("N0");;
		upgradeTitle.text = LocalizationManager.Instance.getText("MENU_UPGRADE_TITLE");
		consumableTitle.text = LocalizationManager.Instance.getText("MENU_CONSUMABLE_TITLE");

	}

	public void showStore()
	{
		powerupCanvas.gameObject.SetActive( false );
		storeCanvas.gameObject.SetActive( true );
	}

	public void closeStore()
	{
		powerupCanvas.gameObject.SetActive( true );
		storeCanvas.gameObject.SetActive( false );
	}

	public void closeMenu()
	{
		StartCoroutine( close() );
	}
	
	IEnumerator close()
	{
		if( !levelLoading )
		{
			SoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			Application.LoadLevel( 3 );
		}
	}
}
