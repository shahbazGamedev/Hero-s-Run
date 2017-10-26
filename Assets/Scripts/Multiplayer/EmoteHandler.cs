﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

//2 valid MP4 videos for testing:
//Tracer video is: 480x458
//McCree video is: 480x384
//Tracer https://media.giphy.com/media/WH1CTjz57LkWc/giphy.mp4?response_id=592291966300b212499a7a14
//McCree https://media.giphy.com/media/NwxtUBYgoz9sI/giphy.mp4?response_id=592292b365a16233448c8f56

public class EmoteHandler : MonoBehaviour {

	[Header("For Sending Emotes")]
 	[SerializeField] GameObject textEmotePrefab;
 	[SerializeField] Transform textEmoteHolder;

	[Header("Configuration")]
	[SerializeField] List<EmoteData> emoteList = new List<EmoteData>();

	// Use this for initialization
	void Start ()
	{
		createTextEmotes();
	}

	#region Text emote creation
	void createTextEmotes()
	{
		for( int i = 0; i < emoteList.Count; i++ )
		{
			createTextEmote( emoteList[i] );
		}
	}

	void createTextEmote( EmoteData ed )
	{
		GameObject go = (GameObject)Instantiate(textEmotePrefab);
		go.transform.SetParent(textEmoteHolder,false);
		Button button = go.GetComponent<Button>();
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() => sendEmote( ed.uniqueID ));
		go.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.getText( ed.textID );
	}
	#endregion

	#region Send and receive emotes
	public void sendEmote( byte uniqueID )
	{
		string senderName = HUDMultiplayer.hudMultiplayer.getLocalPlayerName();

		if( GameManager.Instance.playerDebugConfiguration.getDebugInfoType() == DebugInfoType.EMOTES_TEST )
		{
			//This allows you to send emotes to yourself for testing
			GetComponent<PhotonView>().RPC("emoteRPC", PhotonTargets.All, uniqueID, senderName );
		}
		else
		{
			GetComponent<PhotonView>().RPC("emoteRPC", PhotonTargets.Others, uniqueID, senderName );
		}
	}

	[PunRPC]
	void emoteRPC( byte emoteID, string senderName )
	{
		Debug.Log("emoteRPC " + emoteID + " sent by " + senderName );
		CancelInvoke("hideEmoteAfterDelay");
		EmoteData ed = emoteList.Find(emoteData => emoteData.uniqueID == emoteID);
		if( ed != null )
		{
			GameObject go = HUDMultiplayer.hudMultiplayer.getEmoteGameObjectForPlayerNamed( senderName );
			go.GetComponent<EmoteUI>().displayEmote( ed );
		}
		else
		{
			Debug.Log("EmoteHandler-Emote Data with the unique ID " + emoteID + " was not found. Ignoring." );
		}
	}
	#endregion

	[System.Serializable]
	public class EmoteData
	{
		public byte uniqueID;
		public VideoClip videoClip;
		public string  videoURL;
		public Sprite stillImage;
		public string textID;
		public AudioClip soundByte;
	}
	
}
