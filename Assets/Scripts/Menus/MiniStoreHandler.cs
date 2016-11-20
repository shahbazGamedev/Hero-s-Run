using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MiniStoreHandler : MonoBehaviour {

	[Header("Mini Store")]
	public GameObject saveMeCanvas;
	public Image miniStoreImage;
	public Sprite hero;
	public Sprite heroine;
	public Text titleText;
	public Text suggestionText;
	[Header("Purchase Status Popup")]
	public GameObject purchaseStatusPopup;
	public Text purchaseStatusTitleText;
	public Text purchaseStatusContentText;
	public Text continueButtonText;
	
	
	void Awake()
	{
		titleText.text = LocalizationManager.Instance.getText("MINISTORE_TITLE");
		suggestionText.text = LocalizationManager.Instance.getText("MINISTORE_SUGGESTION");

		if( PlayerStatsManager.Instance.getAvatar() == Avatar.Hero )
		{
			miniStoreImage.sprite = hero;
		}
		else
		{
			miniStoreImage.sprite = heroine;
		}

	}

	public void showMiniStore()
	{
		saveMeCanvas.SetActive( false );
		saveMeCanvas.GetComponent<CanvasGroup>().alpha = 0;
		GetComponent<Animator>().Play("Panel Slide In");
	}

	public void hideMiniStore()
	{
		Invoke("reactivateSaveMeCanvas", 0.5f );
		GetComponent<Animator>().Play("Panel Slide Out");
	}

	void reactivateSaveMeCanvas()
	{
		//Wait until slide out finished
		saveMeCanvas.SetActive( true );
		StartCoroutine( Utilities.fadeInCanvasGroup( saveMeCanvas.GetComponent<CanvasGroup>(), 0.9f ) );
	}

	void showPurchaseStatusPopup( PurchaseStatus purchaseStatus )
	{
		switch (purchaseStatus)
		{
	        case PurchaseStatus.Success:
				purchaseStatusTitleText.text = LocalizationManager.Instance.getText("PURCHASE_STATUS_TITLE_SUCCESS");
				purchaseStatusContentText.text = LocalizationManager.Instance.getText("PURCHASE_STATUS_CONTENT_SUCCESS");
                break;

	        case PurchaseStatus.Cancelled:
				purchaseStatusTitleText.text = LocalizationManager.Instance.getText("PURCHASE_STATUS_TITLE_CANCELLED");
				purchaseStatusContentText.text = LocalizationManager.Instance.getText("PURCHASE_STATUS_CONTENT_CANCELLED");
                break;
  
	        case PurchaseStatus.Error:
				purchaseStatusTitleText.text = LocalizationManager.Instance.getText("PURCHASE_STATUS_TITLE_ERROR");
				purchaseStatusContentText.text = LocalizationManager.Instance.getText("PURCHASE_STATUS_CONTENT_ERROR");
				break;     
		}
		purchaseStatusPopup.SetActive( true );

	}

	public void hidePurchaseStatusPopup()
	{
		purchaseStatusPopup.SetActive( false );
		hideMiniStore();
	}

	public void buyLives( int quantity )
	{
		Debug.Log("buyLives");
		UISoundManager.uiSoundManager.playButtonClick();

		//Grant the purchased lives
		PlayerStatsManager.Instance.increaseLives( quantity );

		//Save the data
		PlayerStatsManager.Instance.savePlayerStats();

		//Show the purchase status popup
		showPurchaseStatusPopup( PurchaseStatus.Success );
	}

}
