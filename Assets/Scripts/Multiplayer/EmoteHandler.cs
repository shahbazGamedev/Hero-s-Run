using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EmoteType
{
	ANIMATED = 0,
	TEXT = 1
}

public class EmoteHandler : MonoBehaviour {

	const float DELAY_BEFORE_HIDING = 3f;

	[Header("Configuration")]
	[SerializeField] List<EmoteData> emoteList = new List<EmoteData>();

	[Header("For Sending Emotes")]
 	[SerializeField] GameObject animatedEmotePrefab;
 	[SerializeField] GameObject textEmotePrefab;
 	[SerializeField] Transform animatedEmoteHolder;
 	[SerializeField] Transform textEmoteHolder;

	[Header("For Receiving Emotes")]
	[SerializeField] Image animatedEmote;
 	[SerializeField] GameObject textEmoteObject;

	// Use this for initialization
	void Start ()
	{
		createAnimatedEmotes();
		createTextEmotes();
	}

	#region Animated emote creation
	void createAnimatedEmotes()
	{
		List<EmoteData> animatedEmoteList = emoteList.FindAll(emoteData => emoteData.type == EmoteType.ANIMATED);
		for( int i = 0; i < animatedEmoteList.Count; i++ )
		{
			createAnimatedEmote( animatedEmoteList[i] );
		}
	}

	void createAnimatedEmote( EmoteData ed )
	{
		GameObject go = (GameObject)Instantiate(animatedEmotePrefab);
		go.transform.SetParent(animatedEmoteHolder,false);
		Button button = go.GetComponent<Button>();
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() => sendEmote( ed.uniqueID ));
		go.GetComponent<EmoteUI>().configureEmote( ed );
	}
	#endregion

	#region Text emote creation
	void createTextEmotes()
	{
		List<EmoteData> textEmoteList = emoteList.FindAll(emoteData => emoteData.type == EmoteType.TEXT);
		for( int i = 0; i < textEmoteList.Count; i++ )
		{
			createTextEmote( textEmoteList[i] );
		}
	}

	void createTextEmote( EmoteData ed )
	{
		GameObject go = (GameObject)Instantiate(textEmotePrefab);
		go.transform.SetParent(textEmoteHolder,false);
		Button button = go.GetComponent<Button>();
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() => sendEmote( ed.uniqueID ));
		go.GetComponent<EmoteUI>().configureEmote( ed );
	}
	#endregion

	#region Send and receive emotes
	public void sendEmote( byte uniqueID )
	{
		GetComponent<PhotonView>().RPC("emoteRPC", PhotonTargets.All, uniqueID );
	}

	[PunRPC]
	void emoteRPC( byte emoteID )
	{
		Debug.Log("emoteRPC " + emoteID );
		CancelInvoke("hideEmoteAfterDelay");
		EmoteData ed = emoteList.Find(emoteData => emoteData.uniqueID == emoteID);
		if( ed != null )
		{
			if( ed.type == EmoteType.ANIMATED )
			{
				animatedEmote.gameObject.GetComponent<EmoteUI>().configureEmote( ed );
				animatedEmote.gameObject.SetActive( true );
				if( ed.animatedSpriteAudio != null ) animatedEmote.GetComponent<AudioSource>().PlayOneShot( ed.animatedSpriteAudio );
			}
			else
			{
				textEmoteObject.GetComponent<EmoteUI>().configureEmote( ed );
				textEmoteObject.SetActive( true );
			}
		}
		else
		{
			Debug.Log("EmoteHandler-Emote Data with the unique ID " + emoteID + " was not found. Ignoring." );
		}
		Invoke( "hideEmoteAfterDelay", DELAY_BEFORE_HIDING );
	}
	#endregion

	void hideEmoteAfterDelay()
	{
		animatedEmote.gameObject.SetActive( false );
		textEmoteObject.SetActive( false );
	}

	[System.Serializable]
	public class EmoteData
	{
		public byte uniqueID;
		public EmoteType type; 
		public Sprite animatedSprite;
		public AudioClip animatedSpriteAudio;
		public string textID;
	}
	
}
