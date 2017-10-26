using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This class displays the emote sent to us by our opponent such as Good Game!
/// It is highly flexible.
/// In all cases, the text will be updated to show Good Game!, Well Played! etc.
/// If there is a sound byte audio clip, it will be played.
/// For the visual aspects, the choice as to what to display is done in the following order:
/// If a video clip is specified, it will be played
/// If there is no video clip, but there is a video URL, the streamed video will be played.
/// If there is no video clip or URL, a still image will be displayed if available.
/// If there is not even a still image, only the text bubble will be shown.
/// </summary>
public class EmoteUI : MonoBehaviour {

 	[SerializeField] Image emoteImage;
 	[SerializeField] RawImage rawImageForVideo;
	[SerializeField] VideoPlayerHandler videoPlayerHandler;
	[SerializeField] TextMeshProUGUI emoteText;
	[SerializeField] AudioSource audioSourceForSoundByte; //Not used by video

	const float DELAY_BEFORE_HIDING = 3.5f;

	public void displayEmote ( EmoteHandler.EmoteData ed )
	{
		gameObject.SetActive( true );
		CancelInvoke( "emptyEmoteTextAfterDelay" );

		//The game object displaying the video will be activated when the video starts playing. See VideoPlayerHandler.
		//If we do not do that, we will see a white texture for a few frame while the video prepares.
		rawImageForVideo.gameObject.SetActive( false );

		//In all cases, set up the text
		emoteText.text = LocalizationManager.Instance.getText( ed.textID );

		//If we have an audio clip, play it
		if( ed.soundByte != null )
		{
			audioSourceForSoundByte.PlayOneShot( ed.soundByte );
		}

		//If we have a local video clip, play that
		if( ed.videoClip != null )
		{
			emoteImage.gameObject.SetActive( false );
			videoPlayerHandler.startVideo( ed.videoClip );
		}
		//If we have a URL for the video, play that
		else if(  !string.IsNullOrEmpty( ed.videoURL ) )
		{
			emoteImage.gameObject.SetActive( false );
			videoPlayerHandler.startVideo( ed.videoURL );
		}
		else if(  ed.stillImage != null )
		{
			emoteImage.sprite = ed.stillImage;
			emoteImage.gameObject.SetActive( true );
		}
		else
		{
			emoteImage.gameObject.SetActive( false );
		}
		Invoke( "emptyEmoteTextAfterDelay", DELAY_BEFORE_HIDING );
	}

	void emptyEmoteTextAfterDelay()
	{
		emoteText.text = string.Empty;
	}


}
