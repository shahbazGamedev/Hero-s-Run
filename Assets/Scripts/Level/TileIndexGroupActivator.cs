using UnityEngine;
using System.Collections;

public class TileIndexGroupActivator : MonoBehaviour {

	public enum TileActivatorType 
	{
		Inactive = 0,
		Equal = 1,
		Less_Than = 2,
		Bigger_Than = 3,
	}

	[Tooltip("The game object to make active if the conditions are met.")]
	public GameObject groupToActivate;
	[Tooltip("The tile index used in the condition.")]
	public int tileIndex = 0;
	[Tooltip("The comparaison type. If EQUAL for example, the group will only be activated if the tileIndex specified is the same as the tile index found in the tile's segment info. This allows you to create behaviors where you have no goblins before tile 10 for example.")]
	public TileActivatorType tileActivatorType = TileActivatorType.Inactive;

	// Use this for initialization
	void Start ()
	{
		int currentTileIndex = GetComponent<SegmentInfo>().tileIndex;

		switch (tileActivatorType)
		{
			case TileActivatorType.Inactive:
			//Do nothing
				break;
			case TileActivatorType.Equal:
				if( currentTileIndex == tileIndex )
				{
					groupToActivate.SetActive( true );
				}
				break;
			case TileActivatorType.Less_Than:
				if( currentTileIndex < tileIndex )
				{
					groupToActivate.SetActive( true );
				}
				break;
			case TileActivatorType.Bigger_Than:
				if( currentTileIndex > tileIndex )
				{
					groupToActivate.SetActive( true );
				}
				break;
		}
		
	}
	
	
}
