using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Uses an AudioSource and a RawImage.
public class AudioWaveFormVisualizer : MonoBehaviour
{

	[SerializeField] int width = 500; 		// texture width 
	[SerializeField] int height = 100; 		// texture height 
	[SerializeField] Color backgroundColor = Color.black; 
	[SerializeField] Color waveformColor = Color.green; 
	[SerializeField] int size = 2048; 		// size of sound segment displayed in texture
	[SerializeField] float amplitudeModifier = 1f; 	// used to boost the amplitude
	
	Color[] blank; 							// blank image array 
	RawImage rawImage; 
	float[] samples; 						// audio samples array
	Texture2D texture;
	AudioSource audioSource;
	
	public void initialize ()
	{ 
		audioSource = GetComponent<AudioSource>();
		rawImage = GetComponent<RawImage>();
		
		// create the samples array 
		samples = new float[size]; 
		
		// create the texture and assign to the RawImage 
		texture = new Texture2D(width, height);
		
		rawImage.texture = texture; 
		
		// create a 'blank screen' image 
		blank = new Color[width * height]; 
		
		for (int i = 0; i < blank.Length; i++)
		{ 
			blank [i] = backgroundColor; 
		}
		//This line will make sure we have our flat wave displayed in the middle.
		getCurrentWave ();
	}
	
	IEnumerator displayAudioWave ( float length )
	{ 
		// refresh the display periodically
		float timeStarted = Time.time;
		do
		{
			getCurrentWave (); 
			yield return new WaitForSeconds (0.1f); 
		} while ( (Time.time - timeStarted) <= length );

		getCurrentWave (); //to make sure we have a flat line once the clip has finished playing
	}

	void getCurrentWave ()
	{ 
		// clear the texture 
		texture.SetPixels (blank, 0); 
		
		// get samples from channel 0 (left) 
		audioSource.GetOutputData (samples, 0); 
		
		// draw the waveform 
		for (int i = 0; i < size; i++)
		{ 
			texture.SetPixel ((int)(width * i / size), (int)(height * (( samples [i] * amplitudeModifier ) + 1f) / 2f), waveformColor);
		}

		// upload to the graphics card 
		texture.Apply (); 
	}

	public void playClip()
	{
		StopAllCoroutines();
		audioSource.Play();
		StartCoroutine( displayAudioWave ( audioSource.clip.length ) );
	}
}
