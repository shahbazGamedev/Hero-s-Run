using UnityEngine;
using System.Collections;

public class characterButton : MonoBehaviour {

	public GameObject frog;
	public GameObject effect;
	
	
	
	private Rect FpsRect ;
	private string frpString;
	
	private GameObject instanceObj;
	public GameObject[] gameObjArray=new GameObject[9];
	public AnimationClip[] AniList  = new AnimationClip[4];
	
	float minimum = 2.0f;
	float maximum = 50.0f;
	float touchNum = 0f;
	string touchDirection ="forward"; 
	private GameObject Villarger_A_Girl_prefab;
	
	// Use this for initialization
	void Start () {
		
		//frog.animation["dragon_03_ani01"].blendMode=AnimationBlendMode.Blend;
		//frog.animation["dragon_03_ani02"].blendMode=AnimationBlendMode.Blend;
		//Debug.Log(frog.GetComponent("dragon_03_ani01"));
		
		//Instantiate(gameObjArray[0], gameObjArray[0].transform.position, gameObjArray[0].transform.rotation);
	}
	
 void OnGUI() {
		
		if (GUI.Button(new Rect(20, 20, 70, 40),"D_Idle")){
		 frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_D_Idle");
	  }
		if (GUI.Button(new Rect(90, 20, 70, 40),"D_Walk")){
		 frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_D_Walk");
	  }
		if (GUI.Button(new Rect(160, 20, 70, 40),"D_Run")){
		 frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_D_Run");
	  }
		
		if (GUI.Button(new Rect(230, 20, 70, 40),"DrawWeapon")){
		 frog.animation.wrapMode= WrapMode.Once;
		  	frog.animation.CrossFade("SC_D_DrawWeapon");
	  }
		 if (GUI.Button(new Rect(300, 20, 70, 40),"D_Standy")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_D_Standy");
			//effect.animation.CrossFade ("test");
	  }
	    if (GUI.Button(new Rect(370, 20, 70, 40),"D_Attack")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_D_Attack");
			//effect.animation.CrossFade ("test");
	  }
		   if (GUI.Button(new Rect(440, 20, 70, 40),"D_Attack01")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_D_Attack01");
	  }

	     if (GUI.Button(new Rect(510, 20, 70, 40),"D_Damage")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_D_Damage");
	  } 
		if (GUI.Button(new Rect(580, 20, 70, 40),"D_Dead")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_D_Dead");
	  }
		if (GUI.Button(new Rect(650, 20, 70, 40),"Fly")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_D_Fly");
	  }
	  if (GUI.Button(new Rect(720, 20, 70, 40),"Idle_Walk")){
		 frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_Idle_walk");
	  }
		
		if (GUI.Button(new Rect(20, 60, 70, 40),"F_FlyMove")){
		 frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_F_FlyMove");
	  }
		if (GUI.Button(new Rect(90, 60, 70, 40),"L_FlyMove")){
		 frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_L_FlyMove");
	  }
		
		if (GUI.Button(new Rect(160, 60, 70, 40),"R_FlyMove")){
		 frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_R_FlyMove");
	  }
		
		
		
	    if (GUI.Button(new Rect(230, 60, 70, 40),"Attack")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_Attack");
			//effect.animation.CrossFade ("test");
	  }
		   if (GUI.Button(new Rect(300, 60, 70, 40),"Attack01")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_Attack01");
	  }
			   if (GUI.Button(new Rect(370, 60, 70, 40),"Curse")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_Curse");
	  }
			   if (GUI.Button(new Rect(440, 60, 70, 40),"Skill")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_Skill");
	  }
	     if (GUI.Button(new Rect(510, 60, 70, 40),"Damage")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_Damage");
	  } 
		if (GUI.Button(new Rect(580, 60, 70, 40),"Dead")){
		  frog.animation.wrapMode= WrapMode.Once;
		  	frog.animation.CrossFade("SC_Dead");
	  }
		
		if (GUI.Button(new Rect(650, 60, 70, 40),"Dance")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("SC_Dance");
	  }
		//	if (GUI.Button(new Rect(370, 20, 70, 40),"Happy")){
		//  frog.animation.wrapMode= WrapMode.Loop;
		//  	frog.animation.CrossFade("Happy");
	//  }
		//	if (GUI.Button(new Rect(440, 20, 70, 40),"Sad")){
		//  frog.animation.wrapMode= WrapMode.Loop;
		//  	frog.animation.CrossFade("Sad");
	  //}
		//	if (GUI.Button(new Rect(510, 20, 140, 40),"GangnamStyle")){
		//  frog.animation.wrapMode= WrapMode.Loop;
		//  	frog.animation.CrossFade("GangnamStyle");
	 // }  
				if (GUI.Button(new Rect(20, 580, 140, 40),"Ver 1.6")){
		 frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Idle");
	  }
		
		
 }
	
	// Update is called once per frame
	void Update () {
		
		//if(Input.GetMouseButtonDown(0)){
		
			//touchNum++;
			//touchDirection="forward";
		 // transform.position = new Vector3(0, 0,Mathf.Lerp(minimum, maximum, Time.time));
			//Debug.Log("touchNum=="+touchNum);
		//}
		/*
		if(touchDirection=="forward"){
			if(Input.touchCount>){
				touchDirection="back";
			}
		}
	*/
		 
		//transform.position = Vector3(Mathf.Lerp(minimum, maximum, Time.time), 0, 0);
	if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
		//frog.transform.Rotate(Vector3.up * Time.deltaTime*30);
	}
	
}
