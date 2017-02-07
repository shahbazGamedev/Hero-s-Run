using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HeroSelectionManager : MonoBehaviour {

	[Header("Hero Selection Manager")]
	[SerializeField] Text confirmButtonText;
	[SerializeField] Text exitButtonText;
	bool levelLoading = false;
	[Header("Active Ability")]
	[SerializeField] Text activeAbilityType;
	[Header("Active Ability")]
	[SerializeField] Text passiveAbilityType;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
		confirmButtonText.text = LocalizationManager.Instance.getText("HERO_SELECTION_CONFIRM");
		exitButtonText.text = LocalizationManager.Instance.getText("CIRCUIT_EXIT");
		//Active
		activeAbilityType.text = LocalizationManager.Instance.getText("ABILITY_TYPE_ACTIVE");
		//Passive
		passiveAbilityType.text = LocalizationManager.Instance.getText("ABILITY_TYPE_PASSIVE");
	}

	public void OnClickShowMatchmaking()
	{
		GameManager.Instance.setGameState( GameState.Matchmaking );
		StartCoroutine( loadScene(GameScenes.Matchmaking) );
	}

	public void OnClickReturnToCircuitSelection()
	{
		StartCoroutine( loadScene(GameScenes.CircuitSelection) );
	}

	IEnumerator loadScene(GameScenes value)
	{
		if( !levelLoading )
		{
			UISoundManager.uiSoundManager.playButtonClick();
			levelLoading = true;
			Handheld.StartActivityIndicator();
			yield return new WaitForSeconds(0);
			SceneManager.LoadScene( (int)value );
		}
	}
	
}
