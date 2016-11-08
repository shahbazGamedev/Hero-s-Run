using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChallengeHUD : MonoBehaviour {

	const float FINAL_POSITION = -55f;
	const float SLIDE_DURATION = 0.3f;
	const float START_X_POSITION = 160f;

	bool hasChallenger = false;
	public Image portrait;
	public Text score;
	public Text label;
	int originalScore; //we decrement this value to 0 as the player is running
	List<ChallengeBoard.Challenge> sortedChallengeListByScore;
	int challengeIndex = 0;
	ChallengeBoard.Challenge activeChallenge;

	// Use this for initialization
	void Start ()
	{
		initializeChallenger();
	}

	void initializeChallenger()
	{
		if( GameManager.Instance.getGameMode() == GameMode.Endless )
		{
			sortedChallengeListByScore = GameManager.Instance.challengeBoard.getChallenges( LevelManager.Instance.getCurrentEpisodeNumber() );

			if( sortedChallengeListByScore.Count > 0 )
			{
				Debug.Log("ChallengeHUD-initializeChallenger: we have " + sortedChallengeListByScore.Count + " challengers." );
				configureChallengerPanel();
			}
		}
	}

	void configureChallengerPanel()
	{
		if( challengeIndex < sortedChallengeListByScore.Count )
		{
			activeChallenge = sortedChallengeListByScore[challengeIndex];
			portrait.GetComponent<FacebookPortraitHandler>().setPortrait( activeChallenge.challengerID );
			originalScore = activeChallenge.score;
			score.text = activeChallenge.score.ToString("N0");
			label.text = activeChallenge.challengerFirstName;
			hasChallenger = true;
			challengeIndex++;
		}
		else
		{
			slideOutChallenger();
			hasChallenger = false;
		}
	}

	void Update()
	{
		if( hasChallenger )
		{
			int currentScore = originalScore - ( LevelManager.Instance.getScore() + PlayerStatsManager.Instance.getDistanceTravelled() );
			if( currentScore < 0 )
			{
				activeChallenge.status = ChallengeStatus.Completed;
				GameManager.Instance.challengeBoard.serializeChallenges();
				//Player has beaten this high score. Configure HUD for next opponent if any
				configureChallengerPanel();
			}
			else
			{
		 		score.text = currentScore.ToString("N0");
			}
		}
	}

	void slideInChallenger()
	{
		if( hasChallenger )
		{
			LeanTween.moveX( GetComponent<RectTransform>(), FINAL_POSITION, SLIDE_DURATION ).setEase(LeanTweenType.easeOutQuad);
		}
	}

	void slideOutChallenger()
	{
		if( hasChallenger )
		{
			LeanTween.moveX( GetComponent<RectTransform>(), START_X_POSITION, SLIDE_DURATION ).setEase(LeanTweenType.easeOutQuad);
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
