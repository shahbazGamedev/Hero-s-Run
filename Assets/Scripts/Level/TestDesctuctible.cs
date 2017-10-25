using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDesctuctible : MonoBehaviour {

	public FracturedObject fracturedObject;
	
	public Transform hitPositionTransform;
	public float hitRadius = 1f;
	public float hitForce = 1f;
	public bool CheckStructureIntegrity = false;
	public float resetDelay = 6f;
   
    void Start()
    {
		#if UNITY_EDITOR
		if( fracturedObject == null ) fracturedObject = GetComponent<FracturedObject>();
		#endif
	}

	void Update()
    {
		#if UNITY_EDITOR
		handleKeyboard();
		if( fracturedObject == null ) GetComponent<FracturedObject>();
		#endif
	}

	void handleKeyboard()
	{
		if ( Input.GetKeyDown (KeyCode.Y) ) 
		{
            fracturedObject.Explode(hitPositionTransform.position, hitForce, hitRadius, false, true, false, CheckStructureIntegrity);
			Invoke("reset", resetDelay );
		}
	}

	void reset()
	{
		fracturedObject.ResetChunks();
	}

}
