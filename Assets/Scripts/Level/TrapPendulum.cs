using UnityEngine;
using System.Collections;

public class TrapPendulum : MonoBehaviour {

	public float angle = 42f;
	public float duration = 1.7f;
	public bool initialDirection = false;
	public GameObject soundSource;

	// Use this for initialization
	void Start ()
	{
		if( initialDirection )
		{
			angle = angle * -1f;
		}

		LeanTween.rotateZ( gameObject, angle, duration ).setEase(LeanTweenType.easeInOutQuad).setOnComplete(rotationEnded).setOnCompleteParam(gameObject);
	}
	
	void rotationEnded( )
	{
		angle = angle * -1f;
		LeanTween.rotateZ( gameObject, angle, duration ).setEase(LeanTweenType.easeInOutQuad).setOnComplete(rotationEnded).setOnCompleteParam(gameObject);
		if( soundSource != null ) soundSource.GetComponent<AudioSource>().Play();
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		LeanTween.resume(gameObject);
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		LeanTween.pause(gameObject);
	}
	
	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Paused )
		{
			LeanTween.pause(gameObject);
		}
		else if( newState == GameState.Normal )
		{
			LeanTween.resume(gameObject);
		}
	}
}
