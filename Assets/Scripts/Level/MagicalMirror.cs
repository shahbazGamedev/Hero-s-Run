using UnityEngine;
using System.Collections;

public class MagicalMirror : MonoBehaviour {

	public float fadeInDuration = 5f;
	public float fadeOutDuration = 5f;
	public float remainFadedDuration = 5f;
	public ParticleSystem sparkle;
	public Animator steel_broken_door;
	public bool hasTriggered = false;

	Material magicalMirrorMaterial;

	// Use this for initialization
	void Awake ()
	{
		magicalMirrorMaterial = GetComponent<MeshRenderer>().material;
		magicalMirrorMaterial.color = new Color( magicalMirrorMaterial.color.r, magicalMirrorMaterial.color.g, magicalMirrorMaterial.color.b, 0f );
	}
	
	void fadeInFarScene()
	{
		hasTriggered = true;
		if( sparkle != null ) sparkle.Play();
		GetComponent<AudioSource>().Play();
		if( steel_broken_door != null ) InvokeRepeating( "makeDoorShudder", 0.3f, 1.7f );
		LeanTween.color( gameObject, new Color( magicalMirrorMaterial.color.r, magicalMirrorMaterial.color.g, magicalMirrorMaterial.color.b, 1f ), fadeInDuration ).setOnComplete(fadeOutFarScene).setOnCompleteParam(gameObject);
	}

	void fadeOutFarScene()
	{
		CancelInvoke();
		LeanTween.color( gameObject, new Color( magicalMirrorMaterial.color.r, magicalMirrorMaterial.color.g, magicalMirrorMaterial.color.b, 0f ), fadeOutDuration ).setDelay(remainFadedDuration).setOnComplete(fadeOutFinished).setOnCompleteParam(gameObject);
	}

	void fadeOutFinished()
	{
		GetComponent<AudioSource>().Stop();
	}

	void makeDoorShudder()
	{
		steel_broken_door.Play("Shudder");
	}

	// Update is called once per frame
	void Update ()
	{
		if( !hasTriggered )
		{
	
			#if UNITY_EDITOR
			// User pressed the left mouse up
			if (Input.GetMouseButtonUp(0))
			{
				MouseButtonUp(0);
			}
			#else
			detectTaps();
			#endif
		}
	}

	void MouseButtonUp(int Button)
	{
		GetHit(Input.mousePosition);
	}

	void detectTaps()
	{
		if ( Input.touchCount > 0 )
		{
			Touch touch = Input.GetTouch(0);
			if( touch.tapCount == 1 )
			{
				if( touch.phase == TouchPhase.Ended  )
				{
					GetHit(Input.mousePosition);
				}
			}
		}
	}

	void GetHit( Vector2 touchPosition )
	{
		// We need to actually hit an object
		RaycastHit hit;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(touchPosition), out hit, 1000))
		{
			print("hit " + hit.collider.name );
			if (hit.collider.name == "Mirror Far Scene" )
			{
				fadeInFarScene();
			}
		}
	}
}
