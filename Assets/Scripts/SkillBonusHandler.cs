using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public sealed class SkillBonusHandler : MonoBehaviour {

	[Header("Skill Bonus")]
	[SerializeField] RectTransform skillBonusHolder;
	[SerializeField] GameObject skillBonusPrefab;

	public static SkillBonusHandler Instance;

	// Use this for initialization
	void Awake ()
	{
		Instance = this;
	}

	#region Not-Coop
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

	/// <summary>
	/// Adds the skill bonus. This only applies when not in coop mode.
	/// </summary>
	/// <param name="skillPoints">Skill points.</param>
	/// <param name="skillTextID">Skill text I.</param>
	public void addSkillBonus( int skillPoints, string skillTextID )
	{
		if( GameManager.Instance.isCoopPlayMode() ) return;

		string localizedSkillText = LocalizationManager.Instance.getText(skillTextID);
		localizedSkillText = string.Format( localizedSkillText, skillPoints );
		//Also update the skill bonus total in player profile
		//so we can convert those bonuses to XP at the end of the race.
		GameManager.Instance.playerProfile.addToSkillBonus( skillPoints );
		StartCoroutine( showBonus( localizedSkillText ) );
	}
	#endregion

	#region Coop
	private Transform getPlayerByPhotonViewID( int photonViewID )
	{
		Transform player = null;
		for( int i = 0; i < PlayerRace.players.Count; i ++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				//We found the player
				player = PlayerRace.players[i].transform;
				break;
			}
		}
		return player;
	}

	public void grantScoreBonus( int bonusPoints, string bonusTextID, int attackerPhotonViewID )
	{
		if( !GameManager.Instance.isCoopPlayMode() ) return;

		Transform attacker = getPlayerByPhotonViewID( attackerPhotonViewID );
		grantScoreBonus( bonusPoints, bonusTextID, attacker );
	}

	public void grantScoreBonus( int bonusPoints, string bonusTextID, Transform attacker )
	{
		if( !GameManager.Instance.isCoopPlayMode() ) return;

		if( attacker != null )
		{
			if( attacker.CompareTag("Player") )
			{
				PlayerMatchData pmd = LevelManager.Instance.getPlayerMatchDataByName( attacker.name );
				if( pmd != null )
				{
					//Grants the attacker one kill and the specified bonus points.
					pmd.score += bonusPoints;
					pmd.kills++;
					if( attacker.GetComponent<PhotonView>().isMine && attacker.GetComponent<PlayerAI>() == null )
					{
						//show a bonus message on the HUD.
						string localizedSkillText = LocalizationManager.Instance.getText(bonusTextID);
						localizedSkillText = string.Format( localizedSkillText, bonusPoints );
						StartCoroutine( showBonus( localizedSkillText ) );
					}
				}
				else
				{
					Debug.LogWarning("SkillBonusHandler-grantScoreBonus: the attacker specified doesn't have player match data: " + attacker.name + " .Not granting bonus." );
				}
			}
			else
			{
				Debug.LogWarning("SkillBonusHandler-grantScoreBonus: the attacker specified is not a player: " + attacker.name + " .Not granting bonus." );
			}
		}
		else
		{
			Debug.LogWarning("SkillBonusHandler-grantScoreBonus: the attacker specified is null. Not granting bonus." );
		}
	}
	#endregion

	#region Shared
	public IEnumerator showBonus( string localizedText )
	{
		//Fade in 0.8 sec,stay 3 sec, fade-out 0.6 sec
		GameObject skillBonus = GameObject.Instantiate( skillBonusPrefab );
		skillBonus.GetComponent<CanvasGroup>().alpha = 0;
		skillBonus.GetComponent<FadeInCanvasGroup>().fadeIn();
		TextMeshProUGUI skillText = skillBonus.GetComponentInChildren<TextMeshProUGUI>();
		skillText.text = localizedText;
		RectTransform skillBonusRectTransform = skillBonus.GetComponent<RectTransform>();
		skillBonusRectTransform.SetParent( skillBonusHolder );
		skillBonusRectTransform.localScale = Vector3.one;
		skillBonusRectTransform.anchoredPosition = new Vector2( 0,0 );
		yield return new WaitForSeconds( 3f );
		LeanTween.moveLocalY( skillBonus, skillBonusRectTransform.anchoredPosition.y + skillBonusRectTransform.sizeDelta.y, 1f );
		skillBonus.GetComponent<FadeInCanvasGroup>().fadeOut();
		Destroy( skillBonus, 1f );
	}
	#endregion
}
