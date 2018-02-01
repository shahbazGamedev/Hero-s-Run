using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public sealed class SkillBonusHandler : MonoBehaviour {

	[Header("Skill (Competition) and Score (COOP) Bonuses")]
	[SerializeField] RectTransform skillBonusHolder;
	[SerializeField] GameObject skillBonusPrefab;
	Queue<GameObject> bonusQueue = new Queue<GameObject>();
	const float BONUS_STAY_TIME = 2.4f; //in seconds
	const float BONUS_FEED_TTL = 3.3f; //in seconds
	float timeOfLastBonus;
	public static SkillBonusHandler Instance;

	// Use this for initialization
	void Awake ()
	{
		Instance = this;
	}

	#region Shared
	void Update()
	{
		#if UNITY_EDITOR
		handleKeyboard();
		#endif
		processQueue();
	}

	private void handleKeyboard()
	{
		//Also support keys for debugging
		if ( Input.GetKeyDown (KeyCode.B) ) 
		{
			addSkillBonus( 69, "SKILL_BONUS_FIREWALL" );
		}
	}

	void processQueue()
	{
		if( bonusQueue.Count > 0 )
		{
			if( Time.time - timeOfLastBonus > BONUS_FEED_TTL )
			{
				StartCoroutine( showBonus( bonusQueue.Dequeue() ) );
				timeOfLastBonus = Time.time;
			}
		}
	}

	void createBonus( string localizedText )
	{
		GameObject bonus = GameObject.Instantiate( skillBonusPrefab );
		bonus.GetComponent<CanvasGroup>().alpha = 0;
		TextMeshProUGUI skillText = bonus.GetComponentInChildren<TextMeshProUGUI>();
		skillText.text = localizedText;
		RectTransform skillBonusRectTransform = bonus.GetComponent<RectTransform>();
		skillBonusRectTransform.SetParent( skillBonusHolder );
		skillBonusRectTransform.localScale = Vector3.one;
		skillBonusRectTransform.anchoredPosition = new Vector2( 0,0 );
		bonusQueue.Enqueue( bonus );
	}

	//Fade in 0.8 sec,stay 2.5 sec, fade-out 0.6 sec
	IEnumerator showBonus( GameObject objectInQueue )
	{
		objectInQueue.GetComponent<FadeInCanvasGroup>().fadeIn();
		yield return new WaitForSeconds( BONUS_STAY_TIME );
		RectTransform rt = objectInQueue.GetComponent<RectTransform>();
		LeanTween.moveLocalY( objectInQueue, rt.anchoredPosition.y + rt.sizeDelta.y, 1f );
		objectInQueue.GetComponent<FadeInCanvasGroup>().fadeOut();
		Destroy( objectInQueue, 0.7f );
	}
	#endregion

	#region Competition
	/// <summary>
	/// Adds the skill bonus. This only applies when not in coop mode.
	/// </summary>
	/// <param name="skillPoints">Skill points.</param>
	/// <param name="skillTextID">Skill text ID.</param>
	public void addSkillBonus( int skillPoints, string skillTextID )
	{
		if( GameManager.Instance.isCoopPlayMode() ) return;

		string localizedSkillText = LocalizationManager.Instance.getText(skillTextID);
		localizedSkillText = string.Format( localizedSkillText, skillPoints );
		//Also update the skill bonus total in player profile
		//so we can convert those bonuses to XP at the end of the race.
		GameManager.Instance.playerProfile.addToSkillBonus( skillPoints );
		createBonus( localizedSkillText );
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

	[PunRPC]
	public void grantComboScoreBonusRPC( int bonusPoints, string bonusTextID, int attackerPhotonViewID, int numberOfKills, bool incrementKills )
	{
		Transform attacker = getPlayerByViewID( attackerPhotonViewID );
		if( attacker != null )
		{
			if( attacker.GetComponent<PhotonView>().isMine )
			{
				grantComboScoreBonus( bonusPoints, bonusTextID, attacker, numberOfKills, incrementKills );
			}
		}
	}

	Transform getPlayerByViewID( int photonViewId )
	{
		Transform player = null;
		for( int i = 0; i < PlayerRace.players.Count; i ++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewId )
			{
				//We found the player
				player = PlayerRace.players[i].transform;
				break;
			}
		}
		return player;
	}

	public void grantScoreBonus( int bonusPoints, string bonusTextID, Transform attacker, bool incrementKills = true )
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
					if( incrementKills ) pmd.kills++;
					if( attacker.GetComponent<PhotonView>().isMine && attacker.GetComponent<PlayerAI>() == null )
					{
						//show a bonus message on the HUD.
						string localizedSkillText = LocalizationManager.Instance.getText(bonusTextID);
						//Debug.Log("SkillBonusHandler-grantScoreBonus: localizedSkillText: " + bonusTextID + " " + localizedSkillText + " " + bonusPoints );

						localizedSkillText = string.Format( localizedSkillText, bonusPoints );
						createBonus( localizedSkillText );
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

	public void grantComboScoreBonus( int bonusPoints, string bonusTextID, Transform attacker, int numberOfKills, bool incrementKills = true )
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
					pmd.score = pmd.score + bonusPoints * numberOfKills;
					if( incrementKills ) pmd.kills = pmd.kills + numberOfKills;
					if( attacker.GetComponent<PhotonView>().isMine && attacker.GetComponent<PlayerAI>() == null )
					{
						//show a bonus message on the HUD.
						string localizedSkillText = LocalizationManager.Instance.getText(bonusTextID);
						localizedSkillText = string.Format( localizedSkillText, bonusPoints * numberOfKills, numberOfKills );
						createBonus( localizedSkillText );
					}
				}
				else
				{
					Debug.LogWarning("SkillBonusHandler-grantComboScoreBonus: the attacker specified doesn't have player match data: " + attacker.name + " .Not granting bonus." );
				}
			}
			else
			{
				Debug.LogWarning("SkillBonusHandler-grantComboScoreBonus: the attacker specified is not a player: " + attacker.name + " .Not granting bonus." );
			}
		}
		else
		{
			Debug.LogWarning("SkillBonusHandler-grantComboScoreBonus: the attacker specified is null. Not granting bonus." );
		}
	}
	#endregion

}
