using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoPlayerHandler : MonoBehaviour {

	[SerializeField] RawImage rawImageForPlayback;
	VideoPlayer videoPlayer;
	AudioSource audioSource;

	void OnEnable()
	{
		videoPlayer = GetComponent<VideoPlayer>();
		audioSource = GetComponent<AudioSource>();
		videoPlayer.prepareCompleted += Prepared;
		videoPlayer.loopPointReached += EndReached;
	}

	void OnDisable()
	{
		videoPlayer.prepareCompleted -= Prepared;
		videoPlayer.loopPointReached -= EndReached;
	}

	public void startVideo ( string videoURL, bool loop = false )
	{
		if( videoPlayer.isPlaying ) videoPlayer.Stop();
		videoPlayer.source = VideoSource.Url;
		videoPlayer.url = videoURL;

		videoPlayer.isLooping = loop;
		audioSource.loop = loop;

		playVideo();
	}

	public void startVideo ( VideoClip videoClip, bool loop = false )
	{
		if( videoPlayer.isPlaying ) videoPlayer.Stop();
		videoPlayer.source = VideoSource.VideoClip;
		videoPlayer.clip = videoClip;

		videoPlayer.isLooping = loop;
		audioSource.loop = loop;

		playVideo();
	}

	void playVideo ()
	{
		videoPlayer.playOnAwake = false;
		audioSource.playOnAwake = false;

		//These lines MUST be called before Prepare() for the audio to play
		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
		videoPlayer.EnableAudioTrack( 0, true );
		videoPlayer.SetTargetAudioSource(0, GetComponent<AudioSource>() );
		audioSource.Pause();

		videoPlayer.Prepare();
	}

	void Prepared( VideoPlayer videoPlayer )
	{
		rawImageForPlayback.texture = videoPlayer.texture;
		videoPlayer.Play();
		//NOTE: can't get audio to work
		audioSource.Play();
	}

	/// <summary>
	/// Called when the end of the video clip is reached.
	/// It is NOT called when the end of a streaming URL video is reached.
	/// </summary>
	/// <param name="videoPlayer">Video player.</param>
	void EndReached( VideoPlayer videoPlayer )
	{
		//print("Video " + videoPlayer.clip.name + " has finished playing." );
	}
}
