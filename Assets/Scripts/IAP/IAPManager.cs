using UnityEngine;
using System.Collections;

public class IAPManager : MonoBehaviour {

	// Use this for initialization
	void Awake ()
	{
	
		Unibiller.onBillerReady += onInitialised;
		Unibiller.Initialise();
	}
		
	private void onInitialised(UnibillState result)
	{
		Debug.Log("IAPManager-onInitialised: Unibiller initilaization result: " + result );
		if( result == UnibillState.SUCCESS )
		{

		}
		else if( result == UnibillState.SUCCESS_WITH_ERRORS )
		{

		}
		else if( result == UnibillState.CRITICAL_ERROR )
		{

		}
	}
	

}
