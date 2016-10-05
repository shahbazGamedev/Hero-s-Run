using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
	List<ChallengeBoard.Challenge> sortedChallengeListByScore;
	int challengeIndex = 0;

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
			ChallengeBoard.Challenge challenge = sortedChallengeListByScore[challengeIndex];
			portrait.GetComponent<FacebookPortraitHandler>().setPortrait( challenge.challengerID );
			originalScore = challenge.score;
			score.text = challenge.score.ToString("N0");
			label.text = challenge.challengerFirstName;
			hasChallenger = true;
			challengeIndex++;
		}
		else
		{
			hasChallenger = false;
		}
	}

	void Update()
	{
		if( hasChallenger )
		{
			int currentScore = originalScore - PlayerStatsManager.Instance.getDistanceTravelled();
			if( currentScore < 0 )
			{
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
