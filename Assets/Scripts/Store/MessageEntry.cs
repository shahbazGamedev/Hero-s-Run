using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MessageEntry : MonoBehaviour {

	[Header("Message Entry")]
	public Image icon;
	public Text message;

	public void initializeMessage( AppRequestData data )
	{
		message.text = data.dataType.ToString() + " " + data.fromFirstName;
		icon.GetComponent<FacebookPortraitHandler>().setPortrait( data.fromID );
	}
}
