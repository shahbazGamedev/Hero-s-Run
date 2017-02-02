using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HeroSelectionManager : MonoBehaviour {

	[Header("World Map Handler")]
	bool levelLoading = false;
	[SerializeField] GameObject abilityPanel;
	[SerializeField] GameObject abilityDetailsPanel;
	[SerializeField] Text confirmButtonText;

	// Use this for initialization
	void Start ()
	{
		confirmButtonText.text = LocalizationManager.Instance.getText("HERO_SELECTION_CONFIRM");
	}

	public void OnClickShowAbilityDetails()
	{
		//UISoundManager.uiSoundManager.playButtonClick();
		abilityPanel.SetActive( false );
		abilityDetailsPanel.GetComponent<Animator>().Play("Panel Slide In");
	}

	public void OnClickHideAbilityDetails()
	{
		//UISoundManager.uiSoundManager.playButtonClick();
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
