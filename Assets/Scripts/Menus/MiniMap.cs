using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class RadarObject
{
	public Image icon { get; set; }
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
	[SerializeField] Text cardFeed; //Used to display the last card played, such as 'Bob played Lightning'
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
		image.transform.SetParent( transform );
		image.rectTransform.localScale = Vector3.one;
		image.sprite = minimapSprite;
		radarObjects.Add( new RadarObject(){ owner = go, icon = image, playerControl = pc } );
	}

	public void removeRadarObject( GameObject go )
	{
		List<RadarObject> newList = new List<RadarObject>();
		for( int i = 0; i < radarObjects.Count; i++ )
		{
			if( radarObjects[i].owner == go )
			{
				Destroy( radarObjects[i].icon );
				continue;
			}
			else
			{
				newList.Add(radarObjects[i]);
			}
		}
		radarObjects.RemoveRange( 0, radarObjects.Count );
		radarObjects.AddRange( newList );
	}

	public void updateCardFeed( string cardPlayerName, CardManager.CardData lastCardPlayed )
	{
		cardFeed.text = cardPlayerName + " played <color=" + CardManager.Instance.getCardColorHexValue( lastCardPlayed.rarity ) + ">" + lastCardPlayed.name + "</color>";
		cardFeedTimeOfLastEntry = Time.time;
	}

	public void updateCardFeed( string cardPlayerName, CardName cardName )
	{
		CardManager.CardData lastCardPlayed = CardManager.Instance.getCardByName( cardName );
		cardFeed.text = cardPlayerName + " played <color=" + CardManager.Instance.getCardColorHexValue( lastCardPlayed.rarity ) + ">" + lastCardPlayed.name + "</color>";
		cardFeedTimeOfLastEntry = Time.time;
	}

	public void updateFeed( string playerName, string message )
	{
		cardFeed.text = playerName + message;
		cardFeedTimeOfLastEntry = Time.time;
	}

	// Update is called once per frame
	void LateUpdate () {
	
		drawRadarDots();
		if( Time.time - cardFeedTimeOfLastEntry > CARD_FEED_TTL ) cardFeed.text = string.Empty;
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
					//The object is off the map. Render it at the edge.
					distToObject = MAX_DISTANCE;
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
					}
					else
					{
						radarObjects[i].icon.overrideSprite = null;
					}
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
}
