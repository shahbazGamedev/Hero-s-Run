using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Zipline : MonoBehaviour {

	public float ziplineExitAngle = 0;
	public float ziplineDuration = 3.5f;

	SegmentInfo.BezierData bezierData;
	//For drawing, each curve is divided into small line segments.
	const int LINE_VERTEX_COUNT = 200;
    LineRenderer lineRenderer;
	float step;

	// Use this for initialization
    void Awake()
	{
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = LINE_VERTEX_COUNT;
		List<SegmentInfo.BezierData> curveList = new List<SegmentInfo.BezierData>();
		curveList = GetComponent<SegmentInfo>().curveList;
 		bezierData = curveList[0];
 		step = 1f/LINE_VERTEX_COUNT;
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
}
