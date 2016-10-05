using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChallengeHUD : MonoBehaviour {

	const float FINAL_POSITION = -55f;
	const float SLIDE_DURATION = 0.3f;
	const float START_X_POSITION = 160f;

	ChallengeBoard cb;
	bool hasChallenger = false;
	public Image portrait;
	public Text score;
	public Text label;
	int originalScore; //we decrement this value to 0 as the player is running

	// Use this for initialization
	void Start ()
	{
		cb = GameManager.Instance.challengeBoard;
		initializeChallenger();
	}

	void initializeChallenger()
	{
		if( GameManager.Instance.getGameMode() == GameMode.Endless )
		{
			if( cb.challengeList.Count > 0 )
			{
				//Make sure we have a challengers for this episode
				portrait.GetComponent<FacebookPortraitHandler>().setPortrait( cb.challengeList[0].challengerID );
				originalScore = cb.challengeList[0].score;
				score.text = cb.challengeList[0].score.ToString("N0");
				hasChallenger = true;
			}
		}
	}

	void Update()
	{
		if( hasChallenger )
		{
			int currentScore = originalScore - PlayerStatsManager.Instance.getDistanceTravelled();
			if( currentScore < 0 ) currentScore = 0 ;
		 	score.text = currentScore.ToString("N0");
		}
	}

	void slideInChallenger()
	{
		if( hasChallenger )
		{
			LeanTween.moveX( GetComponent<RectTransform>(), FINAL_POSITION, SLIDE_DURATION ).setEase(LeanTweenType.easeOutQuad);
		}
	}

	public void hideImmediately()
	{
		if( hasChallenger )
		{
			GetComponent<RectTransform>().anchoredPosition = new Vector2( START_X_POSITION, GetComponent<RectTransform>().anchoredPosition.y );
		}
	}

	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
		PlayerController.playerStateChanged += PlayerStateChange;
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
		PlayerController.playerStateChanged -= PlayerStateChange;
	}

	void PlayerStateChange( CharacterState newState )
	{
		if( newState == CharacterState.Dying )
		{
			hideImmediately();
		}
	}

	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Normal )
		{
			slideInChallenger();
		}
		else
		{
			hideImmediately();
		}
	}
}
