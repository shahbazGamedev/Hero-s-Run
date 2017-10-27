using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillBonusHandler : MonoBehaviour {

	[Header("Skill Bonus")]
	[SerializeField] RectTransform skillBonusHolder;
	[SerializeField] GameObject skillBonusPrefab;

	public static SkillBonusHandler Instance;

	// Use this for initialization
	void Awake ()
	{
		Instance = this;
	}

	void Update()
	{
		#if UNITY_EDITOR
		handleKeyboard();
		#endif
	}

	private void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.B) ) 
		{
			addSkillBonus( 69, "SKILL_BONUS_FIREWALL" );
		}
	}

	public void addSkillBonus( int skillPoints, string skillTextID )
	{
		string localizedSkillText = LocalizationManager.Instance.getText(skillTextID);
		localizedSkillText = string.Format( localizedSkillText, skillPoints );
		//Also update the skill bonus total in player profile
		//so we can convert those bonuses to XP at the end of the race.
		GameManager.Instance.playerProfile.addToSkillBonus( skillPoints );
		StartCoroutine( showSkillBonus( localizedSkillText ) );
	}

	IEnumerator showSkillBonus( string localizedSkillText )
	{
		//Fade in 0.8 sec,stay 2.8 sec, fade-out 0.6 sec
		GameObject skillBonus = GameObject.Instantiate( skillBonusPrefab );
		skillBonus.GetComponent<CanvasGroup>().alpha = 0;
		skillBonus.GetComponent<FadeInCanvasGroup>().fadeIn();
		TextMeshProUGUI skillText = skillBonus.GetComponentInChildren<TextMeshProUGUI>();
		skillText.text = localizedSkillText;
		RectTransform skillBonusRectTransform = skillBonus.GetComponent<RectTransform>();
		skillBonusRectTransform.SetParent( skillBonusHolder );
		skillBonusRectTransform.localScale = Vector3.one;
		skillBonusRectTransform.anchoredPosition = new Vector2( 0,0 );
		yield return new WaitForSeconds( 2.8f );
		LeanTween.moveLocalY( skillBonus, skillBonusRectTransform.anchoredPosition.y + skillBonusRectTransform.sizeDelta.y, 1f );
		skillBonus.GetComponent<FadeInCanvasGroup>().fadeOut();
		Destroy( skillBonus, 1f );
	}
	
}
