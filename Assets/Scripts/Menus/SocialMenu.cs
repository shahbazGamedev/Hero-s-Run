﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SocialMenu : MonoBehaviour {

	[Header("Social Menu")]
	[SerializeField] Transform friendsHolder;
	[SerializeField] GameObject friendEntryPrefab;
	[SerializeField] InputField addFriendInputField;
	[SerializeField] Text addFriendPlaceholderText;

	bool levelLoading = false;

	// Use this for initialization
	void Start () {

		Handheld.StopActivityIndicator();
		createFriendList();
	}

	void createFriendList()
	{
		//Remove previous friends if any. We want to keep the first object which is used to add friends however.
		for( int i = friendsHolder.transform.childCount-1; i >= 1; i-- )
		{
			Transform child = friendsHolder.transform.GetChild( i );
			GameObject.Destroy( child.gameObject );
		}

		List<PlayerFriends.FriendData> friendList = GameManager.Instance.playerFriends.getFriendList();
		for( int i = 0; i < friendList.Count; i++ )
		{
			createFriend( i, friendList[i] );
		}
	}

	void createFriend( int index, PlayerFriends.FriendData fd )
	{
		GameObject go = (GameObject)Instantiate(friendEntryPrefab);
		go.transform.SetParent(friendsHolder,false);
		go.GetComponent<FriendUIDetails>().configureFriend( index, fd );
	}

	/// <summary>
	/// Raises the end edit event when the name of the friend has been entered.
	/// </summary>
	/// <param name="friendName">Friend name.</param>
 	public void OnEndEdit( string friendName )
    {
		if (!string.IsNullOrEmpty(friendName))
		{
			UISoundManager.uiSoundManager.playButtonClick();
			friendName = friendName.Trim();
			if( friendName.Length < UserNameHandler.MINIMUM_USER_NAME_LENGTH )
			{
				addFriendInputField.text = string.Empty;
				addFriendPlaceholderText.text = string.Format( LocalizationManager.Instance.getText( "USER_NAME_NOT_LONG_ENOUGH" ), UserNameHandler.MINIMUM_USER_NAME_LENGTH.ToString() );
				return;
			}
			ChatManager.Instance.chatMessageHandler.sendAddFriendMessage( friendName );
 			addFriendInputField.text = string.Empty;
 			addFriendPlaceholderText.text = LocalizationManager.Instance.getText( "USER_NAME_PLACEHOLDER" );
		}
  	}

	public void OnClickReturnToMainMenu()
	{
		StartCoroutine( loadScene(GameScenes.MainMenu) );
	}

	IEnumerator loadScene(GameScenes value)
	{
		if( !levelLoading )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)value );
		}
	}

	void OnEnable()
	{
		ChatManager.onStatusUpdateEvent += OnStatusUpdateEvent;
		PlayerFriends.onFriendChangedEvent += OnFriendChangedEvent;
	}

	void OnDisable()
	{
		ChatManager.onStatusUpdateEvent -= OnStatusUpdateEvent;
		PlayerFriends.onFriendChangedEvent -= OnFriendChangedEvent;
	}

	void OnStatusUpdateEvent( string userName, int newStatus )
	{
		print("OnStatusUpdateEvent " + userName + " " + newStatus );
		FriendUIDetails fud;
		for( int i = 0; i < friendsHolder.transform.childCount; i++ )
		{
			fud = friendsHolder.transform.GetChild( i ).GetComponent<FriendUIDetails>();
			if( fud != null && fud.user == userName ) fud.configureStatus( newStatus );
		}
	}

	void OnFriendChangedEvent()
	{
		print("OnFriendChangedEvent" );
		createFriendList();
	}
	
}
