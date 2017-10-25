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

	[Tooltip("The TileIndexGroupActivator component should be attached to the tile, not the groupToActivate. The groupToActivate is the game object to make active if the conditions are met.")]
	public GameObject groupToActivate;
	[Tooltip("The tile index used in the condition.")]
	public int tileIndex = 0;
	[Tooltip("The comparaison type. If EQUAL for example, the group will only be activated if the tileIndex specified is the same as the tile index found in the tile's segment info. This allows you to create behaviors where you have no goblins before tile 10 for example.")]
	public TileActivatorType tileActivatorType = TileActivatorType.Inactive;
	[Tooltip("Even if the conditions are met, specify the chance of the group being displayed.")]
	[Range(0, 1f)]
	public float chanceDisplayed = 1f;

	// Use this for initialization
	void OnEnable ()
	{
		if( groupToActivate == null )
		{
			Debug.LogWarning("TileIndexGroupActivator - the group to activate is null. You should either assign it a GameObject or remove this component.");
			return;
		}
		//Assume it will not be activated
		groupToActivate.SetActive( false );
	
		//Or we in the right tile?
		int currentTileIndex = GetComponent<SegmentInfo>().tileIndex;

		switch (tileActivatorType)
		{
			case TileActivatorType.Inactive:
			//Do nothing
				break;
			case TileActivatorType.Equal:
				if( currentTileIndex == tileIndex )
				{
					activateGroup();
				}
				break;
			case TileActivatorType.Less_Than:
				if( currentTileIndex < tileIndex )
				{
					activateGroup();
				}
				break;
			case TileActivatorType.Bigger_Than:
				if( currentTileIndex > tileIndex )
				{
					activateGroup();
				}
				break;
		}
		
	}
	
	//
	void activateGroup()
	{
		if( Random.value <= chanceDisplayed )
		{
			groupToActivate.SetActive( true );
		}
		else
		{
			groupToActivate.SetActive( false );
		}
	}
}
