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
	[SerializeField] Image playerRadarImage;
	[SerializeField] Sprite playerDeadRadarSprite;
	[SerializeField] TextMeshProUGUI cardFeed; //Used to display the last card played, such as 'Bob played Lightning'
	const float MAX_DISTANCE = 72f;
	const float CARD_FEED_TTL = 4f; //in seconds
	float cardFeedTimeOfLastEntry;

	// Use this for initialization
	void Awake () {
	
		Instance = this;
	}

	// Use this for initialization
	public void registerLocalPlayer ( Transform player )
	{
		this.player = player;
	}
	
	public void registerRadarObject( GameObject go, Sprite minimapSprite, PlayerControl pc = null )
	{
		Image image = Instantiate( playerRadarImage );
		Image secondaryImage = image.transform.FindChild("Secondary Image").GetComponent<Image>();
		image.transform.SetParent( transform );
		image.rectTransform.localScale = Vector3.one;
		image.sprite = minimapSprite;
		radarObjects.Add( new RadarObject(){ owner = go, icon = image, playerControl = pc, secondaryIcon = secondaryImage } );
	}

	public void removeRadarObject( GameObject go )
	{
		RadarObject ro = radarObjects.FirstOrDefault(radarObject => radarObject.owner == go);
		if(ro != null) radarObjects.Remove(ro);
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

	// Update is called once per frame
	void LateUpdate () {
	
		drawRadarDots();
		if( Time.time - cardFeedTimeOfLastEntry > CARD_FEED_TTL )
		{
			cardFeed.text = string.Empty;
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
		for(int i = radarObjects.Count - 1; i > -1; i-- )
		{
			if( radarObjects[i].owner != null )
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
