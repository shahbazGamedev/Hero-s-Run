using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class RadarObject
{
	public Image icon { get; set; }
	public GameObject owner { get; set; }
}

public class MiniMap : MonoBehaviour {

	public static MiniMap miniMap = null;
	float mapScale = 1f;
	public Transform player;
	List<RadarObject> radarObjects = new List<RadarObject>();

	// Use this for initialization
	void Awake () {
	
		miniMap = this;
		player = GameObject.FindGameObjectWithTag("Player").transform;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	public void registerRadarObject( GameObject go, Image radarImage )
	{
		Image image = Instantiate( radarImage );
		radarObjects.Add( new RadarObject(){ owner = go, icon = image } );
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

	// Update is called once per frame
	void Update () {
	
		drawRadarDots();
	}

	void drawRadarDots()
	{
		foreach( RadarObject ro in radarObjects )
		{
			Vector3 radarPos = ( ro.owner.transform.position - player.position );
			float distToObject = Vector3.Distance( player.position, ro.owner.transform.position ) * mapScale;
			float deltaY = Mathf.Atan2( radarPos.x, radarPos.z ) * Mathf.Rad2Deg -270 -player.eulerAngles.y;
			radarPos.x = distToObject * Mathf.Cos(deltaY * Mathf.Deg2Rad ) * -1;
			radarPos.z = distToObject * Mathf.Sin( deltaY * Mathf.Deg2Rad );
	
			ro.icon.transform.SetParent( this.transform );
			ro.icon.transform.position = new Vector3( radarPos.x, radarPos.z, 0 ) + this.transform.position;
		}
	}
}
