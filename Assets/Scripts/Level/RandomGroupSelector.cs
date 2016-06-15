using UnityEngine;
using System.Collections;

public class RandomGroupSelector : MonoBehaviour {

	public GameObject group1;
	public GameObject group2;
	public float probabilityGroup1 = 0.5f;

	void OnEnable ()
	{
		if( Random.value <= probabilityGroup1 )
		{
			group1.SetActive( true );
			group2.SetActive( false );
		}
		else
		{
			group1.SetActive( false );
			group2.SetActive( true );
		}
	}
}
