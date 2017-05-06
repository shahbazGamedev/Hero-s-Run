using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StoreNotEnoughGemsPopup : MonoBehaviour {

	[SerializeField] Text titleText;
	[SerializeField] Text descriptionText;
	[SerializeField] Text goToShopButtonText;

	void Start()
	{
		titleText.text = "Not Enough Gems!";
		descriptionText.text = "You don't have enough gems. No worries. You can get some at the shop.";
		goToShopButtonText.text = "Go to Shop";
	}

	public void OnClickGoToShop()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		gameObject.SetActive( false );
		UniversalTopBar.Instance.OnClickShowGemStore();
	}

	public void OnClickHide()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		gameObject.SetActive( false );
	}

}
