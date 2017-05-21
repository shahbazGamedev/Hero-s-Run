using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MultiPurposePopup : MonoBehaviour, IPointerDownHandler {

	public static MultiPurposePopup Instance;
	[SerializeField] GameObject panel;
	[SerializeField] Image topImage;
	[SerializeField] Text contentText;

	void Awake ()
	{
		if(Instance)
		{
			DestroyImmediate(gameObject);
		}
		else
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	public void displayPopup( string contentID, Sprite alternateTopImage = null )
	{
		contentText.text = LocalizationManager.Instance.getText( contentID );
		topImage.overrideSprite = alternateTopImage;
		panel.SetActive( true );
	}

   public void OnPointerDown(PointerEventData data)
    {
		panel.SetActive( false );
    }

}
