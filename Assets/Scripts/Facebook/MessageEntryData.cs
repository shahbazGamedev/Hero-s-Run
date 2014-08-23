using UnityEngine;
using System.Collections;

public class MessageEntryData {

	public Color entryColor;
	public string entryTitle;
	public string entryText;
	public Texture entryIcon;

	public MessageEntryData( RequestDataType requestDataType )
	{
		switch (requestDataType)
		{
		case RequestDataType.Ask_Section_Unlock:
			entryColor 		= Color.magenta;
			entryTitle 		= LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TITLE_HELP_FRIEND" );	//Help your friend!
			entryText 		= LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TEXT_ASK_UNLOCK" );		//Help <first_name> to unlock the next episode!
			entryIcon 		= Resources.Load("GUI/map_icon") as Texture;
			break;
		case RequestDataType.Accept_Section_Unlock:
			entryColor 		= Color.green;
			entryTitle 		= LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TITLE_RECEIVED_HELP" );	//You got help!
			entryText 		= LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TEXT_RECEIVED_HELP" );	//Bob helped you unlock the next episode!
			entryIcon 		= Resources.Load("GUI/map_icon") as Texture;
			break;
		case RequestDataType.Ask_Give_Life:
			entryColor 		= Color.magenta;
			entryTitle 		= LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TITLE_HELP_FRIEND" ); 	//Help your friend!
			entryText 		= LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TEXT_ASK_LIFE" );		//Mary asks you to send a life!
			entryIcon 		= Resources.Load("GUI/life_icon") as Texture;
			break;
		case RequestDataType.Accept_Give_Life:
			entryColor 		= Color.green;
			entryTitle 		= LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TITLE_RECEIVED_GIFT" );	//Gift received
			entryText 		= LocalizationManager.Instance.getText( "MESSAGE_ENTRY_TEXT_RECEIVED_GIFT" );	//Bob offered you a life!
			entryIcon 		= Resources.Load("GUI/life_icon") as Texture;
			break;
		case RequestDataType.Unknown:
			entryColor 		= Color.red;
			entryTitle 		= LocalizationManager.Instance.getText( "MESSAGE_ENTRY_ERROR" );	//Unknown app request data type
			entryText 		= LocalizationManager.Instance.getText( "MESSAGE_ENTRY_ERROR" );	//Unknown app request data type
			entryIcon 		= Resources.Load("GUI/life_icon") as Texture;
			break;
		default:
			Debug.LogWarning("MessageEntryData-unknown data type specified in constructor: " + requestDataType );
			break;
		}
			
	}
}
