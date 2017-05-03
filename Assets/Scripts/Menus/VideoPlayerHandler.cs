using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
		GetComponent<VideoPlayer>().url = "http://www.quirksmode.org/html5/videos/big_buck_bunny.mp4"; //This video has audio
		GetComponent<VideoPlayer>().prepareCompleted += Prepared;
		//These lines MUST be called before Prepare() for the audio to play
		GetComponent<VideoPlayer>().audioOutputMode = VideoAudioOutputMode.AudioSource;
		GetComponent<VideoPlayer>().EnableAudioTrack( 0, true );
		GetComponent<VideoPlayer>().SetTargetAudioSource(0, GetComponent<AudioSource>() );
		GetComponent<VideoPlayer>().Prepare();
	}

	void Prepared( VideoPlayer videoPlayer )
	{
		GetComponent<VideoPlayer>().Play();
		GetComponent<AudioSource>().Play();
	}
}
