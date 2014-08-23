using UnityEngine;
using System.Collections;

public class SpawnStumbles : MonoBehaviour {
	
	static GameObject shortStumble = Resources.Load("Obstacles/Stumble Log") as GameObject; //one lane
	static GameObject longStumble  = Resources.Load("Obstacles/Stumble Log Long") as GameObject;	//two lane
	bool allowStumbles = true; //used for debugging
	
	// Use this for initialization
	void Start ()
	{
		GameObject prefab;

		//There is a percentage chance that a Stumble will be spawned
		float rd = Random.value;
		float heightOffset = -0.24f; //sink the log into the ground
		float offset = 0;
		
		if( rd < 0.1f && allowStumbles )
		{
			//Yes, we should spawn a stumble
			
			//Decide if we should spawn a short or a long one
			rd = Random.Range( 0f, 1f );
			
			if( rd > 0.75f )
			{
				//Spawn a long one
				prefab = longStumble;
				offset = 0.75f; 
			}
			else
			{
				//Spawn a short one
				prefab = shortStumble;
				offset = 1.5f; 
				
			}
			//50% chance it will spawn on the right side and 50% chance on the left side
			Vector3 pos = new Vector3( 0,0,0 );
			if( Random.value <= 0.5f )
			{
				offset = offset * -1; 
			}

			//If the Y angle of the corridor is 0 or 180 degrees, the lanes are along the X=axis.
			//If the Y angle of the corridor is 90 or -90 degrees, the lanes are along the Z-axis.
			//There are sometime small rounding errors In Unity that can make angles change slightly
			//such as 90.0001f instead of 90f. This is why we use Mathf.Floor.
			float corridorY = Mathf.Floor( transform.eulerAngles.y );
			if ( corridorY == 90f || corridorY == -90f || corridorY == 270f || corridorY == -270f)
			{
				pos = new Vector3( transform.position.x, transform.position.y + heightOffset, transform.position.z + offset );
			}
			else if( corridorY == 0f || corridorY == 180f )
			{
				pos = new Vector3( transform.position.x + offset, transform.position.y + heightOffset, transform.position.z );	
			}
			GameObject go = (GameObject)Instantiate(prefab, pos, Quaternion.Euler(0, transform.eulerAngles.y, 0));
			go.transform.parent = transform;
		}
	}

}
