using UnityEngine;
using System.Collections;

/*
This class is used to randomly decide if an object will be active or not.
This is mainly used in prefabs to ensure that a specific object is not always visible in all prefab instances so that
they do not look all identical.
If the random number is less or equal than the chanceDisplayed number, the game object will be visible.
*/
public class RandomActivator : MonoBehaviour {
	
	[SerializeField] float chanceDisplayed = 0.3f;
	
	void OnEnable ()
	{
		if( Random.value <= chanceDisplayed )
		{
			gameObject.SetActive( true );
		}
		else
		{
			gameObject.SetActive( false );
		}
	
	}
	
}
