using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class RadarObject
{
	public Image icon { get; set; }
	public Image secondaryIcon { get; set; }
	public float secondaryIconTTL { get; set; }
	public float secondaryIconTimeDisplayStarted { get; set; }
	public GameObject owner { get; set; }
	public PlayerControl playerControl { get; set; }
}

/// <summary>
/// Mini map. The local player is not represented. His position is in the center of the minimap.
/// If the diameter is 160 pixels, the map edge is 72 meters from the center.
/// Objects further than 72 meters from the local player get drawn at the edge.
/// If a player is dead, his icon changes temporarily to a skull.
/// </summary>
public class MiniMap : MonoBehaviour {

	public static MiniMap Instance = null;
	float mapScale = 1f;
	Transform player;
	List<RadarObject> radarObjects = new List<RadarObject>();
	[SerializeField] Image playerMinimapPrefab;
	[SerializeField] Sprite playerDeadSprite;
	[SerializeField] Sprite playerFarSprite;
	[SerializeField] TextMeshProUGUI cardFeed; //Used to display the last card played, such as 'Bob played Lightning'
	[SerializeField] TextMeshProUGUI cardFeed2; //Used to display reflected cards
	const float MAX_DISTANCE = 78f;
	[SerializeField] float distanceToDrawOnTheEdge = 98f;
	const float CARD_FEED_TTL = 5f; //in seconds
	const float CARD_FEED_TTL2 = 5f; //in seconds
	float cardFeedTimeOfLastEntry;
	float cardFeedTimeOfLastEntry2;
	[SerializeField] RectTransform mapWithPeriphery;
	[SerializeField] RectTransform mapWithoutPeriphery;
	#region Level Map
	[SerializeField] RectTransform levelMap;
	[SerializeField] Image tileMinimapPrefab;
	float tileSize = 0;
	#endregion

	// Use this for initialization
	void Awake () {
	
		Instance = this;
		tileSize = LevelManager.Instance.getSelectedCircuit().tileSize;
	}

	public void registerLocalPlayer ( Transform player )
	{
		this.player = player;
	}
	
	public void registerRadarObject( GameObject go, Sprite minimapSprite, PlayerControl pc = null )
	{
		Image image = Instantiate( playerMinimapPrefab );
		Image secondaryImage = image.transform.FindChild("Secondary Image").GetComponent<Image>();
		image.transform.SetParent( mapWithPeriphery );
		image.rectTransform.localScale = Vector3.one;
		image.sprite = minimapSprite;
		radarObjects.Add( new RadarObject(){ owner = go, icon = image, playerControl = pc, secondaryIcon = secondaryImage } );
	}

	public void registerTileObject( string tileName, Vector3 tilePosition, Sprite minimapSprite, float tileYRotation, int tileDepth )
	{
		tileYRotation = Mathf.Floor( tileYRotation );
		int tileDepthOverOne = tileDepth - 1;
		Image image = Instantiate( tileMinimapPrefab );
 		//Don't use the exact same name as the tile as this will cause issues with Find
		//because two objects will have the same name.
		image.name = "Tile Icon " + tileName;
		image.transform.SetParent( levelMap );
		image.rectTransform.localScale = Vector3.one;
		image.sprite = minimapSprite;
		image.GetComponent<RectTransform>().localEulerAngles = new Vector3( 0, 0, -tileYRotation );
		
		Vector3 radarPos = tilePosition;
		float distToObject = Vector3.Distance( Vector3.zero, tilePosition ) * mapScale;
		float deltaY = Mathf.Atan2( radarPos.x, radarPos.z ) * Mathf.Rad2Deg -270;
		radarPos.x = distToObject * Mathf.Cos(deltaY * Mathf.Deg2Rad ) * -1;
		radarPos.z = distToObject * Mathf.Sin( deltaY * Mathf.Deg2Rad );

		//For a square 1 x 1 tile the center of the tile corresponds to the middle of the tile as expected.
		//However, for a rectangular 1 x 2 tile, the center is off by half a tile size.
		if( tileYRotation == 0 )
		{
			image.rectTransform.anchoredPosition = new Vector2( radarPos.x, radarPos.z + (tileDepthOverOne * tileSize * 0.5f)  );
		}
		else
		{
			image.rectTransform.anchoredPosition = new Vector2( radarPos.x + (tileDepthOverOne * tileSize * 0.5f), radarPos.z );
		}
		image.rectTransform.sizeDelta = new Vector2( tileSize, tileSize * tileDepth );
	}

	public void updateLevelMapPosition()
	{
		//levelMap.rotation = Quaternion.Euler( 0, 0, -player.eulerAngles.y );
		levelMap.anchoredPosition = new Vector2( -player.position.x, -player.position.z );
	}

	public void removeRadarObject( GameObject go )
	{
		RadarObject ro = radarObjects.FirstOrDefault(radarObject => radarObject.owner == go);
		if(ro != null) radarObjects.Remove(ro);
	}

	public void changeAlphaOfRadarObject( PlayerControl pc, float alpha )
	{
		RadarObject ro = radarObjects.Find(radarObject => radarObject.playerControl == pc);
		if( ro != null )
		{
			ro.icon.color = new Color(ro.icon.color.r, ro.icon.color.g, ro.icon.color.b, alpha ) ;
			ro.secondaryIcon.color = new Color(ro.icon.color.r, ro.icon.color.g, ro.icon.color.b, alpha ) ;
		}
	}

	public void displayMessage( string heroName, CardName cardName )
	{
		CardManager.CardData lastCardPlayed = CardManager.Instance.getCardByName( cardName );
		displayMessage( heroName, lastCardPlayed );
	}

	public void displayMessage( string heroName, CardManager.CardData lastCardPlayed )
	{
		string localizedCardName = LocalizationManager.Instance.getText( "CARD_NAME_" + lastCardPlayed.name.ToString().ToUpper() );
		string message = string.Format( LocalizationManager.Instance.getText("MINIMAP_CARD_PLAYED"), heroName, CardManager.Instance.getCardColorHexValue( lastCardPlayed.rarity ), localizedCardName );
		addMessage( message );
	}

	public void displayMessage( string message )
	{
		addMessage( message );
	}

	void addMessage( string message )
	{
		cardFeed.text = message;
		cardFeedTimeOfLastEntry = Time.time;
	}

	public void displayMessage2( string message, Color textColor )
	{
		addMessage2( message, textColor );
	}

	void addMessage2( string message, Color textColor )
	{
		cardFeed2.color = textColor;
		cardFeed2.text = message;
		cardFeedTimeOfLastEntry2 = Time.time;
	}

	// Update is called once per frame
	void LateUpdate ()
	{
		//The minimap will get instantiated before the player. We need the player in order to update the minimap.
		if( player == null ) return;

		updateLevelMapPosition();
		drawRadarDots();
		if( Time.time - cardFeedTimeOfLastEntry > CARD_FEED_TTL )
		{
			cardFeed.text = string.Empty;
		}
		if( Time.time - cardFeedTimeOfLastEntry2 > CARD_FEED_TTL2 )
		{
			cardFeed2.text = string.Empty;
		}
	}

	public void changeColorOfRadarObject( PlayerControl pc, Color newColor )
	{
		RadarObject ro = radarObjects.Find(radarObject => radarObject.playerControl == pc);
		if( ro != null )
		{
			ro.icon.color = newColor;
		}
		else
		{
			Debug.LogError("MiniMap-changeColorOfRadarObject: radar object for " + pc.name + " was not found." );
		}
	}

	public void changeColorOfRadarObject( GameObject owner, Color newColor )
	{
		RadarObject ro = radarObjects.Find(radarObject => radarObject.owner == owner);
		if( ro != null )
		{
			ro.icon.color = newColor;
		}
		else
		{
			Debug.LogError("MiniMap-changeColorOfRadarObject: radar object for " + owner.name + " was not found." );
		}
	}

	public void overrideRadarObjectIcon( GameObject owner, Sprite overrideSprite )
	{
		RadarObject ro = radarObjects.Find(radarObject => radarObject.owner == owner);
		if( ro != null )
		{
			ro.icon.overrideSprite = overrideSprite;
		}
		else
		{
			Debug.LogError("MiniMap-overrideRadarObjectIcon: radar object for " + owner.name + " was not found." );
		}
	}

	void drawRadarDots()
	{
		for(int i = radarObjects.Count - 1; i > -1; i-- )
		{
			if( radarObjects[i].owner != null )
			{
				Vector3 radarPos = ( radarObjects[i].owner.transform.position - player.position );
				float distToObject = Vector3.Distance( player.position, radarObjects[i].owner.transform.position ) * mapScale;
				float deltaY = Mathf.Atan2( radarPos.x, radarPos.z ) * Mathf.Rad2Deg -270;

				if( radarObjects[i].playerControl )
				{
					//Handle players
					//Is the player dead?
					if( radarObjects[i].playerControl.getCharacterState() == PlayerCharacterState.Dying )
					{
						//Yes, he is dead
						//Is the player far?
						if( distToObject > MAX_DISTANCE )
						{
							//Yes, the player is far.
							//Render the icon on the periphery of the minimap.
							distToObject = distanceToDrawOnTheEdge;
							radarPos.x = distToObject * Mathf.Cos(deltaY * Mathf.Deg2Rad ) * -1;
							radarPos.z = distToObject * Mathf.Sin( deltaY * Mathf.Deg2Rad );

							//A dead far player uses the far sprite.
							radarObjects[i].icon.overrideSprite = playerFarSprite;

							//We need to change the parent. mapWithoutPeriphery has a mask.
							//For the icon to be visble on the periphery, we need to change its parent to an object without a mask: mapWithPeriphery.
							radarObjects[i].icon.transform.SetParent( mapWithPeriphery );

							//When an icon is on the periphery, we want it to face the center of the minimap.
							radarObjects[i].icon.rectTransform.rotation = Quaternion.Euler( 0, 0, Mathf.Atan2( radarPos.z, radarPos.x ) * Mathf.Rad2Deg -90 );
						}
						else
						{
							//No, he is not far
							//A dead nearby player uses the dead player sprite.
							radarPos.x = distToObject * Mathf.Cos(deltaY * Mathf.Deg2Rad ) * -1;
							radarPos.z = distToObject * Mathf.Sin( deltaY * Mathf.Deg2Rad );
							radarObjects[i].icon.overrideSprite = playerDeadSprite;
							radarObjects[i].icon.transform.SetParent( mapWithoutPeriphery );
							radarObjects[i].icon.rectTransform.rotation = Quaternion.identity;

						}
						//In all cases, hide the secondary icon when a player is dead.
						radarObjects[i].secondaryIcon.gameObject.SetActive( false );
					}
					else
					{
						//No, the player is alive
						//Is the player far?
						if( distToObject > MAX_DISTANCE )
						{
							//Yes, the player is far.
							//Render the icon on the periphery of the minimap.
							distToObject = distanceToDrawOnTheEdge;
							radarPos.x = distToObject * Mathf.Cos(deltaY * Mathf.Deg2Rad ) * -1;
							radarPos.z = distToObject * Mathf.Sin( deltaY * Mathf.Deg2Rad );

							//An alive far player uses the player far sprite
							radarObjects[i].icon.overrideSprite = playerFarSprite;

							//We need to change the parent. mapWithoutPeriphery has a mask.
							//For the icon to be visble on the periphery, we need to change its parent to an object without a mask: mapWithPeriphery.
							radarObjects[i].icon.transform.SetParent( mapWithPeriphery );

							//When an icon is on the periphery, we want it to face the center of the minimap.
							radarObjects[i].icon.rectTransform.rotation = Quaternion.Euler( 0, 0, Mathf.Atan2( radarPos.z, radarPos.x ) * Mathf.Rad2Deg -90 );
						}
						else
						{
							//No, he is not far
							//A nearby alive player uses his initial icon.
							radarPos.x = distToObject * Mathf.Cos(deltaY * Mathf.Deg2Rad ) * -1;
							radarPos.z = distToObject * Mathf.Sin( deltaY * Mathf.Deg2Rad );
							radarObjects[i].icon.overrideSprite = null;
							radarObjects[i].icon.transform.SetParent( mapWithoutPeriphery );
							radarObjects[i].icon.rectTransform.rotation = Quaternion.identity;
						}
					}

					//Only players have secondary icons.
					//Handle secondary icons which may have expired.
					if( radarObjects[i].secondaryIcon.gameObject.activeSelf && ( Time.time - radarObjects[i].secondaryIconTimeDisplayStarted ) > radarObjects[i].secondaryIconTTL ) radarObjects[i].secondaryIcon.gameObject.SetActive( false );
				}
				else
				{
					//Handle other icons like those for devices.
					if( distToObject > MAX_DISTANCE )
					{
						//The object (like a teleporter) is off the map. Simply hide it.
						radarObjects[i].icon.gameObject.SetActive( false );
						continue;
					}
					else
					{
						radarObjects[i].icon.gameObject.SetActive( true );
					}
				}

				//Position icon on the minimap
				radarObjects[i].icon.rectTransform.anchoredPosition = new Vector2( radarPos.x, radarPos.z );

			}
			else
			{
				//The owner of the radar object is null.
				//This will happen if one of our opponents disconnects.
				//Simply remove that entry from the radarObjects
				Destroy( radarObjects[i].icon );
		        radarObjects.RemoveAt(i);
			}
		}
	}

	[PunRPC]
	void minimapRPC( string heroName, int card )
	{
		//Display a message under the minimap.
		displayMessage( heroName, (CardName)card );
	}

	public void displaySecondaryIcon( int photonViewID, int card, float duration )
	{
		GetComponent<PhotonView>().RPC( "minimapSecondaryRPC", PhotonTargets.All, photonViewID, card, duration );
	}

	[PunRPC]
	void minimapSecondaryRPC( int photonViewID, int card, float duration )
	{
		//Some cards have a secondary icon on the top-left of the player icon.
		PlayerControl pc = getPlayerControl( photonViewID );
		RadarObject ro = radarObjects.Find(radarObject => radarObject.playerControl == pc);
		if( ro != null )
		{
			Sprite secondarySprite = CardManager.Instance.getCardByName( (CardName)card ).secondaryIcon;
			if( secondarySprite != null )
			{
				ro.secondaryIcon.sprite = secondarySprite;
				ro.secondaryIcon.gameObject.SetActive( true );
				ro.secondaryIconTTL = duration;
				ro.secondaryIconTimeDisplayStarted = Time.time;
			}
		}
		else
		{
			Debug.LogWarning("MiniMap-minimapSecondaryRPC: radar object for " + pc.name + " was not found." );
		}
	}

	/// <summary>
	/// This method sends a reflectMessageRPC message. Once this message is received, two messages will be displayed on the HUD.
	/// The first message will say that this player reflected this card to the caster.
	/// The second message will say that the caster got affected by his own spell.
	/// </summary>
	/// <param name="casterPhotonViewID">Caster photon view ID.</param>
	/// <param name="cardName">Card name.</param>
	/// <param name="playerWithReflectPhotonViewID">Player with reflect photon view ID.</param>
	public void reflectMessage( int casterPhotonViewID, int cardName, int playerWithReflectPhotonViewID )
	{
		GetComponent<PhotonView>().RPC( "reflectMessageRPC", PhotonTargets.All, casterPhotonViewID, cardName, playerWithReflectPhotonViewID );
	}

	[PunRPC]
	void reflectMessageRPC( int casterPhotonViewID, int cardName, int playerWithReflectPhotonViewID )
	{
		if( cardFeed2 == null ) return;
		print("Minimap-reflectMessageRPC: casterPhotonViewID: " + casterPhotonViewID + " cardName " + (CardName)cardName + " playerWithReflectPhotonViewID: " + playerWithReflectPhotonViewID );
		string nameOfPlayerWithReflect = getPlayerControl( playerWithReflectPhotonViewID ).name;
		string localizedCardName = LocalizationManager.Instance.getText( "CARD_NAME_" + ((CardName)cardName).ToString().ToUpper() );
		string nameOfPlayerWhoCastSpell = getPlayerControl( casterPhotonViewID ).name;
		displayMessage2( string.Format( "{0} reflected {1} activated by {2}", nameOfPlayerWithReflect, localizedCardName, nameOfPlayerWhoCastSpell ), Color.white );
	}

	public void hideSecondaryIcon( GameObject go )
	{
		RadarObject ro = radarObjects.FirstOrDefault(radarObject => radarObject.owner == go);
		if(ro != null) ro.secondaryIcon.gameObject.SetActive( false );
	}

	PlayerControl getPlayerControl( int photonViewID )
	{
		PlayerControl playerControl = null;
		for( int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().viewID == photonViewID )
			{
				playerControl = PlayerRace.players[i].GetComponent<PlayerControl>();
				break;
			}
		}
		return playerControl;
	}

}
