using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum StoreTab {
	Store = 1,
	Shop = 2
}

public enum PurchaseStatus {
	Success = 0,
	Cancelled = 1,
	Error = 2
}

public class StoreManager : MonoBehaviour {

	public const float HARD_CURRENCY_TO_SOFT_CURRENCY_RATIO = 20; //1 gem is equal to 20 coins
	[Header("General")]
	public static StoreManager Instance = null;
	[SerializeField]  GameObject storeParent;
	[SerializeField]  RectTransform storeVerticalContent;

	// Use this for initialization
	void Awake ()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start ()
	{
		//Adjust the content height so that everything fits
		LayoutElement[] elements = storeVerticalContent.GetComponentsInChildren<LayoutElement>();
		float contentHeight = 0;
		for( int i =0; i < elements.Length; i++ )
		{
			contentHeight += elements[i].minHeight;
		}
		//storeVerticalContent.sizeDelta = new Vector2( storeVerticalContent.sizeDelta.x, contentHeight );
	}

	public void showStore( StoreTab selectedTab )
	{
		storeParent.SetActive( true );
		if( selectedTab == StoreTab.Store )
		{
			//to be re-implemented
		}
		else if( selectedTab == StoreTab.Shop )
		{	
			//to be re-implemented
		}
	}

	public void closeStore()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		storeParent.SetActive( false );
	}

}
