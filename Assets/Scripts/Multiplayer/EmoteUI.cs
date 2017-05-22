using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmoteUI : MonoBehaviour {

 	[SerializeField] Image emoteImage;
 	[SerializeField] RawImage rawImageForVideo;
	[SerializeField] VideoPlayerHandler videoPlayerHandler;
	[SerializeField] TextMeshProUGUI emoteText;
	[SerializeField] AudioSource audioSourceForSoundByte; //Not used by video

	public void displayEmote ( EmoteHandler.EmoteData ed )
	{
		gameObject.SetActive( true );

		//In all cases, set up the text
		emoteText.text = LocalizationManager.Instance.getText( ed.textID );

		//If we have an audio clip, play it
		if( ed.soundByte != null )
		{
			print("Sound byte " + ed.soundByte.name );
			audioSourceForSoundByte.PlayOneShot( ed.soundByte );
		}

		//If we have a local video clip, play that
		if( ed.videoClip != null )
		{
			emoteImage.gameObject.SetActive( false );
			rawImageForVideo.gameObject.SetActive( true );
			videoPlayerHandler.startVideo( ed.videoClip );
		}
		//If we have a URL for the video, play that
		else if(  !string.IsNullOrEmpty( ed.videoURL ) )
		{
			emoteImage.gameObject.SetActive( false );
			rawImageForVideo.gameObject.SetActive( true );
			videoPlayerHandler.startVideo( ed.videoURL );
		}
		else if(  ed.stillImage != null )
		{
			emoteImage.sprite = ed.stillImage;
			emoteImage.gameObject.SetActive( true );
			rawImageForVideo.gameObject.SetActive( false );
		}
		else
		{
			emoteImage.gameObject.SetActive( false );
			rawImageForVideo.gameObject.SetActive( false );
		}
	}
}
