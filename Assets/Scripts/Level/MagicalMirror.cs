using UnityEngine;
using System.Collections;

public class MagicalMirror : MonoBehaviour {

	public float fadeInDuration = 5f;
	public float fadeOutDuration = 5f;
	public float remainFadedDuration = 5f;
	public ParticleSystem sparkle;

	Material magicalMirrorMaterial;

	// Use this for initialization
	void Awake ()
	{
		magicalMirrorMaterial = GetComponent<MeshRenderer>().material;
		magicalMirrorMaterial.color = new Color( magicalMirrorMaterial.color.r, magicalMirrorMaterial.color.g, magicalMirrorMaterial.color.b, 0f );
	}
	
	void Update ()
	{
		if ( Input.GetKeyDown (KeyCode.F) ) 
		{
			print("fading");
			fadeInFarScene();
		}
	}

	void fadeInFarScene()
	{
		if( sparkle != null ) sparkle.Play();
		LeanTween.color( gameObject, new Color( magicalMirrorMaterial.color.r, magicalMirrorMaterial.color.g, magicalMirrorMaterial.color.b, 1f ), fadeInDuration ).setOnComplete(fadeOutFarScene).setOnCompleteParam(gameObject);
	}

	void fadeOutFarScene()
	{
		LeanTween.color( gameObject, new Color( magicalMirrorMaterial.color.r, magicalMirrorMaterial.color.g, magicalMirrorMaterial.color.b, 0f ), fadeOutDuration ).setDelay(remainFadedDuration);
	}

}
