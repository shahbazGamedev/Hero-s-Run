using UnityEngine;
using System.Collections;

public class CoinHandler : MonoBehaviour {

	public enum StarPackHeight
	{
		Normal = 0,		//About 0.85 meters from ground. You do not need to jump.
		High = 1,		//About 2.85 meters from ground. You need to jump to reach.
		Very_High = 2,	//About 5 meters from ground. You need to double jump to reach.
		Keep_Current_Value = 3	//Keep the value specified in TileReset
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
			if( starPackHeight != StarPackHeight.Keep_Current_Value )
			{
				//Place the star pack at the right height
				//Important: make sure star game object have the ignoreRaycast layer
				RaycastHit hit;
				float groundHeight = 0;
				if (Physics.Raycast(new Vector3(transform.position.x,transform.parent.position.y + 10f,transform.position.z), Vector3.down, out hit, 20.0F ))
				{
					groundHeight = hit.point.y;
					groundHeight = groundHeight + getStarPackHeight();
					transform.position = new Vector3( transform.position.x, groundHeight, transform.position.z);
				}
				else
				{
					Debug.LogError("CoinHandler: Start - There is no ground under the star named, " + gameObject.name );
				}
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
				return 2.25f;
			case StarPackHeight.Very_High:
				return 4.5f;
			default:
				return 0.85f;
		}
	}

}
