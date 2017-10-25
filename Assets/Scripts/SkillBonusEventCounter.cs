using System.Collections;
using UnityEngine;

public class SkillBonusEventCounter : MonoBehaviour {

	//Event setup
	//Text ID of skill bonus awarded if successfull. For example: SKILL_BONUS_DOUBLE_KILL
	public string skillBonusTextID;
	//Number of skill bonus points to award if successfull. For example: 25
	public int awardedSkillBonusPoints;
	//Number of events needed to be successfull. This number should be greated than 1 or else there is no value in using this class. For example: 2
	public int nbrEventsRequired;
	//Allocated time to get nbrEventsRequired. For example: 3 seconds
	public float duration;

	//Event tracking
	//Current number of events
	public int currentNbrEvents = 0;
	Coroutine eventCounterCoroutine = null;

	public void initialize( string skillBonusTextID, int awardedSkillBonusPoints, int nbrEventsRequired, float duration )
	{
		this.skillBonusTextID = skillBonusTextID;
		this.awardedSkillBonusPoints = awardedSkillBonusPoints;
		this.nbrEventsRequired = nbrEventsRequired;
		this.duration = duration;
	}

	public void resetTimer()
	{
		if( eventCounterCoroutine != null )
		{
			StopCoroutine( eventCounterCoroutine );
			eventCounterCoroutine = null;
		}
		currentNbrEvents = 0;
		Debug.LogWarning ("SkillBonusEventCounter-resetTimer for " + skillBonusTextID );
	}

	public void incrementCounter()
	{
		currentNbrEvents++;
		Debug.LogWarning ("SkillBonusEventCounter-incrementCounter: " + skillBonusTextID + " " + currentNbrEvents + "/" + nbrEventsRequired );
		if( currentNbrEvents == 1 )
		{
			//Start the timer
			if( eventCounterCoroutine != null ) StopCoroutine( eventCounterCoroutine );
			eventCounterCoroutine = StartCoroutine( startTimer() );
		}
		validateResult();
	}

	IEnumerator startTimer()
	{
		yield return new WaitForSeconds( duration );
		resetTimer();
	}

	void validateResult()
	{
		if( currentNbrEvents == nbrEventsRequired )
		{
			//Success!
			//Allocate skill bonus.
			Debug.LogWarning ("SkillBonusEventCounter-validateResult: Awarding for " + skillBonusTextID + " " + awardedSkillBonusPoints );
			SkillBonusHandler.Instance.addSkillBonus( awardedSkillBonusPoints, skillBonusTextID );
			resetTimer();
		}
	}

}

