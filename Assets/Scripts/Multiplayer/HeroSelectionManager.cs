using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroSelectionManager : MonoBehaviour {

	[Header("World Map Handler")]
	[SerializeField] GameObject abilityPanel;
	[SerializeField] GameObject abilityDetailsPanel;

	// Use this for initialization
	void Start ()
	{
		
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
	
}
