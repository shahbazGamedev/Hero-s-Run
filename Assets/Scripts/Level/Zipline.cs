using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Zipline : MonoBehaviour {

	SegmentInfo.BezierData bezierData;
	//For drawing, each curve is divided into small line segments.
	const int LINE_VERTEX_COUNT = 200;
    LineRenderer lineRenderer;
	float step;
	List<GameObject> coinsList = new List<GameObject>();
	[SerializeField] bool addCoins = true;
	[SerializeField] GameObject starPrefab;
	[SerializeField] int distanceBetweenCoins = 12;
	[SerializeField] float distanceBelowLine = 1f;

	// Use this for initialization
    void Awake()
	{
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.numPositions = LINE_VERTEX_COUNT;
		List<SegmentInfo.BezierData> curveList = new List<SegmentInfo.BezierData>();
		curveList = GetComponent<SegmentInfo>().curveList;
 		bezierData = curveList[0];
 		step = 1f/LINE_VERTEX_COUNT;
	}

    void addCoinsBelowZipline()
	{
		coinsList.Clear();
		//Only add coins if we are not in multiplayer
		if( addCoins && !GameManager.Instance.isMultiplayer() )
		{
			GameObject go;
			Vector3 starToLineOffset = new Vector3( 0, distanceBelowLine, 0 );
			//Start at 1 to avoid having coins immediately next to the start
			for(int i=1; i < LINE_VERTEX_COUNT; i = i + distanceBetweenCoins )
			{
				float t = i * step;
				Vector3 toPosition = Utilities.Bezier3( bezierData.bezierStart.position, bezierData.bezierControl1.position, bezierData.bezierControl2.position, bezierData.bezierEnd.position, t ) - starToLineOffset;
				go = (GameObject)Instantiate(starPrefab, toPosition, Quaternion.identity );
				go.transform.parent = transform;
				coinsList.Add( go );
			}
		}
    }
	
    void Update()
	{
		Vector3 fromPosition = bezierData.bezierStart.position;
		lineRenderer.SetPosition(0, fromPosition);
		for(int i=1; i < LINE_VERTEX_COUNT; i++ )
		{
			float t = i * step;
			Vector3 toPosition = Utilities.Bezier3( bezierData.bezierStart.position, bezierData.bezierControl1.position, bezierData.bezierControl2.position, bezierData.bezierEnd.position, t );
    		lineRenderer.SetPosition(i, toPosition);

		}
    }

	void OnEnable()
	{
		addCoinsBelowZipline();
	}

	void OnDisable()
	{
		for( int i =0; i < coinsList.Count; i++ )
		{
			GameObject.Destroy( coinsList[i] );
			coinsList[i] = null;
		}
		coinsList.Clear();
	}


}
