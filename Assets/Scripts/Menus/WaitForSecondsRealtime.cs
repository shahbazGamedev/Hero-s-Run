using UnityEngine;

//wait using unscaled time
public class WaitForSecondsRealtime : CustomYieldInstruction
{

	private float waitTime;
	
	public WaitForSecondsRealtime(float time)
	{
		waitTime = Time.realtimeSinceStartup + time;
	}

	public override bool keepWaiting
	{
		get { return Time.realtimeSinceStartup < waitTime; }
	}


	//sample usage
	//yield return new WaitForSecondsRealtime(delay);
}
