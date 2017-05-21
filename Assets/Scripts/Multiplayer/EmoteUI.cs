using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmoteUI : MonoBehaviour {

 	[SerializeField] Image animatedImage;
 	[SerializeField] TextMeshProUGUI emoteText;

	public void configureEmote ( EmoteHandler.EmoteData ed )
	{
		if( ed.type == EmoteType.ANIMATED )
		{
			animatedImage.sprite = ed.animatedSprite;
		}
		else
		{
			emoteText.text = LocalizationManager.Instance.getText( ed.textID );
		}
	}
}
