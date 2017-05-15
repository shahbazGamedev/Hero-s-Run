using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StoreNotEnoughGemsPopup : MonoBehaviour {

	[SerializeField] Text titleText;
	[SerializeField] Text descriptionText;
	[SerializeField] Text goToShopButtonText;
	[SerializeField] ScrollRect horizontalScrollview;

	void Start()
	{
		titleText.text = LocalizationManager.Instance.getText( "CURRENCY_POPUP_NOT_ENOUGH_GEMS" );
		descriptionText.text = LocalizationManager.Instance.getText( "CURRENCY_POPUP_GET_GEMS_AT_SHOP" );
		goToShopButtonText.text = LocalizationManager.Instance.getText( "CURRENCY_BUTTON_GO_TO_SHOP" );
	}

	public void Show()
	{
		//Disable scrolling while popup is displayed
		horizontalScrollview.enabled = false;
		gameObject.SetActive( true );
	}

	public void OnClickGoToShop()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		gameObject.SetActive( false );
		//Re-enable scrolling
		horizontalScrollview.enabled = true;
		UniversalTopBar.Instance.OnClickShowGemStore();
	}

	public void OnClickHide()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		gameObject.SetActive( false );
		//Re-enable scrolling
		horizontalScrollview.enabled = true;
	}

}
