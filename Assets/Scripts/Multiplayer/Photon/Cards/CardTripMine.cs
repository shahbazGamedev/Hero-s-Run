using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Trip Mine card is a Rare card with 11 levels. The mine appears a few meters in front of the player on the ground. The duration of the mine depends on the level of the caster.
/// The player is immune to its effect. If an opponent passes near without jumping, he triggers an explosion.
/// </summary>
public class CardTripMine : Card {

	[SerializeField]  string prefabName;
	Vector3 offset = new Vector3( 0, 0, 10f );

	public void activateCard ( int photonViewId, int level )
	{
		if( isAllowed( photonViewId ) ) this.photonView.RPC("cardTripMineMasterRPC", PhotonTargets.MasterClient, level, photonViewId );	
	}

	#region Methods only running on master client
	[PunRPC]
	void cardTripMineMasterRPC( int level, int photonViewID )
	{
		//Get the transform of the player who activated the card
		Transform playerTransform = getPlayerTransform( photonViewID );

		//Spawn a mine on the floor a few meters in front of the player
		Vector3 tripMinePosition = playerTransform.TransformPoint( offset );
		Quaternion tripMineRotation = playerTransform.rotation;

		CardManager.CardData cd = CardManager.Instance.getCardByName( cardName );
		object[] data = new object[3];

		//We want the caster to be immune to the spell
		data[0] = playerTransform.name;

		//We want the trip mine to disappear after a while
		data[1] = cd.getCardPropertyValue( CardPropertyType.DURATION, level );

		//We want to transmit the blast radius
		data[2] = cd.getCardPropertyValue( CardPropertyType.RADIUS, level );

		PhotonNetwork.InstantiateSceneObject( prefabName, tripMinePosition, tripMineRotation, 0, data );
	}
	#endregion

	public bool isAllowed( int photonViewID )
	{
		//Find out which player activated the card
		GameObject playerGameObject = null;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				playerGameObject = (GameObject)PhotonNetwork.player.TagObject;
			}
		}

		//Verify that the player is not spawning a mine inside the finish line trigger
		Vector3 position = playerGameObject.transform.TransformPoint( offset );

		GameObject boxColliderObject = GameObject.FindGameObjectWithTag("Finish Line");
		//If the tile with the finish line is not active, boxColliderObject will be null, so check for that.
		if( boxColliderObject != null )
		{
			BoxCollider boxCollider = boxColliderObject.GetComponent<BoxCollider>();
			if( boxCollider.bounds.Contains( position ) )
			{
				//Don't allow it
				return false;
			}
			else
			{
				return true;
			}
		}
		else
		{
			//If boxColliderObject is null, that means the tile with the finish line is not yet active and therefore, we are far from the finish line.
			//In this case, return true.
			return true;
		}
	}
}
