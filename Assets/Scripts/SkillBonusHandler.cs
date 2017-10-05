using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
			StartCoroutine( showSkillBonus( "Trap Activated +25 XP" ) );
		}
	}

	public IEnumerator showSkillBonus( string bonusText )
	{
		//Fade in 0.8 sec,stay 2.6 sec, fade-out 0.6 sec
		GameObject skillBonus = GameObject.Instantiate( skillBonusPrefab );
		skillBonus.GetComponent<CanvasGroup>().alpha = 0;
		skillBonus.GetComponent<FadeInCanvasGroup>().fadeIn();
		RectTransform skillBonusRectTransform = skillBonus.GetComponent<RectTransform>();
		skillBonusRectTransform.SetParent( skillBonusHolder );
		skillBonusRectTransform.localScale = Vector3.one;
		skillBonusRectTransform.anchoredPosition = new Vector2( 0,0 );
		yield return new WaitForSeconds( 2.6f );
		LeanTween.moveLocalY( skillBonus, skillBonusRectTransform.anchoredPosition.y + skillBonusRectTransform.sizeDelta.y, 1f );
		skillBonus.GetComponent<FadeInCanvasGroup>().fadeOut();
		Destroy( skillBonus, 1f );
	}
	
}
