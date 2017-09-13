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
	[SerializeField] Image tileMinimapPrefab;
	[SerializeField] Sprite playerDeadRadarSprite;
	[SerializeField] TextMeshProUGUI cardFeed; //Used to display the last card played, such as 'Bob played Lightning'
	[SerializeField] TextMeshProUGUI cardFeed2; //Used to display reflected cards
	const float MAX_DISTANCE = 78f;
	const float CARD_FEED_TTL = 5f; //in seconds
	const float CARD_FEED_TTL2 = 5f; //in seconds
	float cardFeedTimeOfLastEntry;
	float cardFeedTimeOfLastEntry2;
	Queue<RadarObject> tileQueue = new Queue<RadarObject>();
	float tileSize = 0;
	[SerializeField] RectTransform levelMap;

	// Use this for initialization
	void Awake () {
	
		Instance = this;
		tileSize = LevelManager.Instance.getSelectedCircuit().tileSize;
	}

	// Use this for initialization
	public void registerLocalPlayer ( Transform player )
	{
		this.player = player;
	}
	
	public void registerRadarObject( GameObject go, Sprite minimapSprite, PlayerControl pc = null )
	{
		Image image = Instantiate( playerMinimapPrefab );
		Image secondaryImage = image.transform.FindChild("Secondary Image").GetComponent<Image>();
		image.transform.SetParent( transform );
		image.rectTransform.localScale = Vector3.one;
		image.sprite = minimapSprite;
		radarObjects.Add( new RadarObject(){ owner = go, icon = image, playerControl = pc, secondaryIcon = secondaryImage } );
	}

	public void registerTileObject( GameObject go, Sprite minimapSprite, float tileYRotation, int tileDepth )
	{
		tileYRotation = Mathf.Floor( tileYRotation );
		int tileDepthOverOne = tileDepth - 1;
		Image image = Instantiate( tileMinimapPrefab );
		image.name = go.name;
		Image secondaryImage = image.transform.FindChild("Secondary Image").GetComponent<Image>();
		image.transform.SetParent( levelMap );
		image.rectTransform.localScale = Vector3.one;
		secondaryImage.gameObject.SetActive( false );
		image.sprite = minimapSprite;
		//RadarObject ro = new RadarObject(){ owner = go, icon = image, playerControl = null, secondaryIcon = secondaryImage };
		image.GetComponent<RectTransform>().localEulerAngles = new Vector3( 0, 0, -tileYRotation );
		//radarObjects.Add( ro );
		//tileQueue.Enqueue( ro );
		Vector3 radarPos = ( go.transform.position - Vector3.zero );
		float distToObject = Vector3.Distance( Vector3.zero, go.transform.position ) * mapScale;
		float playerEulerAnglesY = 0;
		float deltaY = Mathf.Atan2( radarPos.x, radarPos.z ) * Mathf.Rad2Deg -270 -playerEulerAnglesY;
		radarPos.x = distToObject * Mathf.Cos(deltaY * Mathf.Deg2Rad ) * -1;
		radarPos.z = distToObject * Mathf.Sin( deltaY * Mathf.Deg2Rad );

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
		//Vector3 radarPos = ( Vector3.zero - player.position );
		//float distToObject = Vector3.Distance( player.position, Vector3.zero ) * mapScale;
		//float playerEulerAnglesY = player.eulerAngles.y;
		//float deltaY = Mathf.Atan2( radarPos.x, radarPos.z ) * Mathf.Rad2Deg -270 -playerEulerAnglesY;
		//radarPos.x = distToObject * Mathf.Cos(deltaY * Mathf.Deg2Rad ) * -1;
		//radarPos.z = distToObject * Mathf.Sin( deltaY * Mathf.Deg2Rad );
		//levelMap.anchoredPosition = new Vector2( radarPos.x, radarPos.z );
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

	public void updateTopmostTile( Transform newTile )
	{
		return; //code not finished
		//Dequeue
		RadarObject bottomMostTile = tileQueue.Dequeue();

		//Update the tile object
		bottomMostTile.owner = newTile.gameObject;

		//Adjust the image size based on the tile depth.
		//A tile depth of 1 is 50 meters, a depth of 2, is a 100 meters, etc.
		//One pixel is one meter
		adjustTileImageSize( bottomMostTile.icon.GetComponent<RectTransform>(), newTile );

		//Update sprite
		bottomMostTile.icon.sprite = newTile.GetComponent<SegmentInfo>().tileSprite;

		//Update rotation
		bottomMostTile.icon.GetComponent<RectTransform>().localEulerAngles = new Vector3( 0, 0, -newTile.eulerAngles.y );

		//Enqueue
		tileQueue.Enqueue(bottomMostTile);
	}

	private void adjustTileImageSize( RectTransform tileImage, Transform newTile )
	{
		float tileRotationY = Mathf.Floor ( newTile.eulerAngles.y );
		//Tile is facing straight. Adjust height.
		if( tileRotationY == 0 )
		{
			tileImage.sizeDelta = new Vector2( tileSize, newTile.GetComponent<SegmentInfo>().tileDepth * tileSize );
		}
		//Tile is facing right.
		else if( tileRotationY == 90f || tileRotationY == -270f )
		{
			tileImage.sizeDelta = new Vector2( newTile.GetComponent<SegmentInfo>().tileDepth * tileSize, tileSize );
		}
		//Tile is facing left.
		else if( tileRotationY == -90f || tileRotationY == 270f )
		{
			tileImage.sizeDelta = new Vector2( newTile.GetComponent<SegmentInfo>().tileDepth * tileSize, tileSize );
		}
	}

	public void inititalizedStartTiles( Transform[] firstThreeTiles )
	{
		return; //code not finished

		//top of queue positioned at top of minimap
		//registerTileObject( firstThreeTiles[2].gameObject, firstThreeTiles[2].GetComponent<SegmentInfo>().tileSprite, firstThreeTiles[2].eulerAngles.y );
		//registerTileObject( firstThreeTiles[1].gameObject, firstThreeTiles[1].GetComponent<SegmentInfo>().tileSprite, firstThreeTiles[1].eulerAngles.y );
		//registerTileObject( firstThreeTiles[0].gameObject, firstThreeTiles[0].GetComponent<SegmentInfo>().tileSprite, firstThreeTiles[0].eulerAngles.y );
		
		GameObject tileMinusOne = new GameObject();
		tileMinusOne.transform.position = new Vector3( 0, 0, -50f );
		//registerTileObject( tileMinusOne, firstThreeTiles[0].GetComponent<SegmentInfo>().tileSprite, firstThreeTiles[0].eulerAngles.y );

		GameObject tileMinusTwo = new GameObject();
		tileMinusTwo.transform.position = new Vector3( 0, 0, -100f );
		//registerTileObject( tileMinusTwo, firstThreeTiles[0].GetComponent<SegmentInfo>().tileSprite, firstThreeTiles[0].eulerAngles.y );
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
	void LateUpdate () {
	
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

	void drawRadarDots()
	{
		updateLevelMapPosition();
		for(int i = radarObjects.Count - 1; i > -1; i-- )
		{
			if( radarObjects[i].owner != null && player != null )
			{
				Vector3 radarPos = ( radarObjects[i].owner.transform.position - player.position );
				float distToObject = Vector3.Distance( player.position, radarObjects[i].owner.transform.position ) * mapScale;
				if( distToObject > MAX_DISTANCE )
				{
					if( radarObjects[i].playerControl != null )
					{
						//The player is off the map. Render him at the edge.
						distToObject = MAX_DISTANCE;
					}
					else
					{
						//The object (like a teleporter) is off the map. Simply hide it.
						radarObjects[i].icon.gameObject.SetActive( false );
						continue;
					}
				}
				else
				{
					radarObjects[i].icon.gameObject.SetActive( true );
				}
				float deltaY = Mathf.Atan2( radarPos.x, radarPos.z ) * Mathf.Rad2Deg -270 -player.eulerAngles.y;
				radarPos.x = distToObject * Mathf.Cos(deltaY * Mathf.Deg2Rad ) * -1;
				radarPos.z = distToObject * Mathf.Sin( deltaY * Mathf.Deg2Rad );
				radarObjects[i].icon.rectTransform.anchoredPosition = new Vector2( radarPos.x, radarPos.z );
				if( radarObjects[i].playerControl )
				{
					if( radarObjects[i].playerControl.getCharacterState() == PlayerCharacterState.Dying )
					{
						radarObjects[i].icon.overrideSprite = playerDeadRadarSprite;
						//Also hide the secondary icon
						radarObjects[i].secondaryIcon.gameObject.SetActive( false );
					}
					else
					{
						radarObjects[i].icon.overrideSprite = null;
					}
					//Handle secondary icons which may have expired
					if( radarObjects[i].secondaryIcon.gameObject.activeSelf && ( Time.time - radarObjects[i].secondaryIconTimeDisplayStarted ) > radarObjects[i].secondaryIconTTL ) radarObjects[i].secondaryIcon.gameObject.SetActive( false );
				}
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
