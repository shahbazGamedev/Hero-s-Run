using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HeroSelectionManager : MonoBehaviour {

	[Header("World Map Handler")]
	bool levelLoading = false;
	[SerializeField] AbilityDetails abilityDetails;
	[SerializeField] GameObject abilityPanel;
	[SerializeField] GameObject abilityDetailsPanel;
	[SerializeField] Text confirmButtonText;
	[SerializeField] Text exitButtonText;

	// Use this for initialization
	void Start ()
	{
		Handheld.StopActivityIndicator();
		confirmButtonText.text = LocalizationManager.Instance.getText("HERO_SELECTION_CONFIRM");
		exitButtonText.text = LocalizationManager.Instance.getText("CIRCUIT_EXIT");
	}

	public void OnClickShowActiveAbilityDetails()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		abilityDetails.configureActiveAbilityDetails();
		abilityPanel.SetActive( false );
		abilityDetailsPanel.GetComponent<Animator>().Play("Panel Slide In");
	}

	public void OnClickShowPassiveAbilityDetails()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		abilityDetails.configurePassiveAbilityDetails();
		abilityPanel.SetActive( false );
		abilityDetailsPanel.GetComponent<Animator>().Play("Panel Slide In");
	}

	public void OnClickHideAbilityDetails()
	{
		UISoundManager.uiSoundManager.playButtonClick();
		Invoke("displayAbilityPanel", 1.1f);
		abilityDetailsPanel.GetComponent<Animator>().Play("Panel Slide Out");
	}

	public void displayAbilityPanel()
	{
		//Wait until the slide out is finished before displaying the ability panel
		abilityPanel.SetActive( true );
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
