﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SegmentInfo : MonoBehaviour {
	
	public GameObject tile;
	public TileType tileType;
	//Does this tile use straight corridors or bezier curves?
	public bool usesBezierCurve = false;
	//A tile can have multiple Cubic bezier curves.
	public List<BezierData> curveList = new List<BezierData>();
	//For drawing, each curve is divided into small line segments.
	public const int CURVE_DISTANCE_STEPS = 200;
	//This flag is used to avoid tileEntranceCrossed being called multiple time which can happen with onTriggerEnter
	public bool entranceCrossed = false;

	public bool isCheckpoint = false;

	//Display the bezier curve(s) if any.
	void OnDrawGizmos ()
	{
		if( usesBezierCurve )
		{
			Gizmos.color = Color.red;
			SegmentInfo.BezierData bezierData;
			float step = 1f/CURVE_DISTANCE_STEPS;
			for( int i=0; i < curveList.Count; i++ )
			{
				bezierData = curveList[i];
				Vector3 fromPosition = bezierData.bezierStart.position;
				for(int j=0; j < CURVE_DISTANCE_STEPS; j++ )
				{
					float t = j * step;
					Vector3 toPosition = Utilities.Bezier3( bezierData.bezierStart.position, bezierData.bezierControl1.position, bezierData.bezierControl2.position, bezierData.bezierEnd.position, t );
					Gizmos.DrawLine ( fromPosition, toPosition );
					fromPosition = toPosition;
		
				}
			}
		}
	}
	[System.Serializable]
	public class BezierData
	{
		public Transform bezierStart;
		public Transform bezierControl1;
		public Transform bezierControl2;
		public Transform bezierEnd;
	}

}
