using UnityEngine;
using System.Collections;

public class MagicalMirror : MonoBehaviour {

	/*The magical mirror is composed of two quads. The one in front is showing the remote scene and the one behind it is a simple mirror.
	The material on the remote scene quad starts off with an alpha of 0 and the alpha goes to 1 when fadeInFarScene() is called therefore obscuring the mirror.
	*/
	public float fadeInDuration = 5f;
	public float fadeOutDuration = 5f;
	public float remainFadedDuration = 5f;
	public ParticleSystem sparkle;
	public Animator steel_broken_door;
	public TasteOfHellSequence tasteOfHellSequence;
	bool hasTriggered = false;
	bool allowPlayerActivation = false;

	Material mirrorFarSceneMaterial;

	// Use this for initialization
	void Awake ()
	{
		mirrorFarSceneMaterial = GetComponent<MeshRenderer>().material;
		mirrorFarSceneMaterial.color = new Color( mirrorFarSceneMaterial.color.r, mirrorFarSceneMaterial.color.g, mirrorFarSceneMaterial.color.b, 0f );
	}
	
	void fadeInFarScene()
	{
		hasTriggered = true;
		tasteOfHellSequence.remoteScene.SetActive( true );
		if( sparkle != null ) sparkle.Play();
		GetComponent<AudioSource>().Play();
		if( steel_broken_door != null ) InvokeRepeating( "makeDoorShudder", 0.3f, 1.7f );
		LeanTween.color( gameObject, new Color( mirrorFarSceneMaterial.color.r, mirrorFarSceneMaterial.color.g, mirrorFarSceneMaterial.color.b, 1f ), fadeInDuration ).setOnComplete(fadeOutFarScene).setOnCompleteParam(gameObject);
	}

	void fadeOutFarScene()
	{
		CancelInvoke();
		LeanTween.color( gameObject, new Color( mirrorFarSceneMaterial.color.r, mirrorFarSceneMaterial.color.g, mirrorFarSceneMaterial.color.b, 0f ), fadeOutDuration ).setDelay(remainFadedDuration).setOnComplete(fadeOutFinished).setOnCompleteParam(gameObject);
	}

	void fadeOutFinished()
	{
		GetComponent<AudioSource>().Stop();
		tasteOfHellSequence.visionEnded();
	}

	public void allowActivation()
	{
		allowPlayerActivation = true;
	}

	void makeDoorShudder()
	{
		steel_broken_door.Play("Shudder");
	}

	// Update is called once per frame
	void Update ()
	{
		if( allowPlayerActivation && !hasTriggered )
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
			if (hit.collider.name == "Mirror Far Scene" )
			{
				fadeInFarScene();
			}
		}
	}
}
