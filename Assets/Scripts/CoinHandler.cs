using UnityEngine;
using System.Collections;

public class CoinHandler : MonoBehaviour {

	public enum StarPackHeight
	{
		Normal = 0,		//About 0.85 meters from ground. You do not need to jump.
		High = 1,		//About 2.85 meters from ground. You need to jump to reach.
		Very_High = 2	//About 5.2 meters from ground. You need to double jump to reach. 
	}

	public float chanceDisplayed = 0.3f;
	public int totalValueOfCoins = 0;
	public StarPackHeight starPackHeight = StarPackHeight.Normal;

	void Start ()
	{

		float chanceAvailable = chanceDisplayed * GameManager.Instance.getGlobalCoinMultiplier();
		if( Random.value <= chanceAvailable )
		{
			gameObject.SetActive( true );
			CoinManager.realNumberCoinsSpawned = CoinManager.realNumberCoinsSpawned + totalValueOfCoins;
			//Place the star pack at the right height
			//Important: make sure star game object have the ignoreRaycast layer
			RaycastHit hit;
			float groundHeight = 0;
			print ("STAR ground pos Y " + transform.position.y );
			if (Physics.Raycast(new Vector3(transform.position.x,transform.parent.position.y + 10f,transform.position.z), Vector3.down, out hit, 12.0F ))
			{
				groundHeight = 10f - hit.distance;
				print ("STAR ground height " + groundHeight + " " +  transform.parent.name );
				groundHeight = groundHeight + getStarPackHeight();
				print ("STAR ground height ADJUSTED " + groundHeight + " ground pos Y " + transform.position.y);
				transform.position = new Vector3( transform.position.x, groundHeight, transform.position.z);
			}
		}
		else
		{
			gameObject.SetActive( false );
		}
		
	}

	float getStarPackHeight()
	{
		switch( starPackHeight )
		{
			case StarPackHeight.Normal:
				return 0.85f;
			case StarPackHeight.High:
				return 2.85f;
			case StarPackHeight.Very_High:
				return 5.2f;
			default:
				return 0.85f;
		}
	}

}
