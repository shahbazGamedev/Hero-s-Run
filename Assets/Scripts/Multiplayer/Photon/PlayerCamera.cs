using UnityEngine;
using System.Collections;
using Cinemachine;

//If the player starts off on a Start tile, the camera will be looking at the front of player and do a rotation when the player starts running.
//However, if the player is not on a Start tile and is starting at a Checkpoint, we want the camera to look at the back of the player (and therefore, there is no need for a rotation when the player starts running).
public enum CutsceneType 
{
	Start = 0,
	CullisGate = 1,
	Checkpoint = 2,
	Troll = 3,
	OpeningSequence = 4,
	MagicGate = 5,
	SummonSkeletons = 6,
	Ziplining = 7
}

public enum CameraState 
{
	Normal = 0,
	Locked = 1,
	Cutscene = 2
}

/*
This camera smoothes out rotation around the y-axis and height.
Horizontal Distance to the target is always fixed.

There are many different ways to smooth the rotation but doing it this way gives you a lot of control over how the camera behaves.

For every of those smoothed values we calculate the wanted value and the current value.
Then we smooth it using the Lerp function.
Then we apply the smoothed values to the transform's position.
*/
public class PlayerCamera : Photon.PunBehaviour {
	

	private Transform mainCamera;
	public Transform cutsceneCamera;
	const float DEFAULT_CAMERA_X_ROT = -7f;
	public float cameraXrotation = DEFAULT_CAMERA_X_ROT;
	public const float DEFAULT_MAIN_CAMERA_FOV = 74f;
	public const float DEFAULT_CUTSCENE_FOV = 37f;

	// The distance in the x-z plane to the target
	public const float DEFAULT_DISTANCE = 6f;
	float distance = DEFAULT_DISTANCE;
	// the height we want the camera to be above the target
	public const float DEFAULT_HEIGHT = 2.5f;
	float height = DEFAULT_HEIGHT;
	// How much we 
	public const float DEFAULT_HEIGHT_DAMPING = 3f;
	public float heightDamping = DEFAULT_HEIGHT_DAMPING;
	float rotationDamping = 16f;

	//Used for camera shake
	//For now, the code assumes the player is not
	//moving (because he is dead for example), when the shake occurs.
	private float shake_decay = 0;
	private float shake_intensity = 0;
	public bool isCameraLocked = true;
	
	private CameraState cameraState = CameraState.Cutscene;

	//Transform the camera is following. By default, it is the player.
	Transform cameraTarget;

	//Use 0 if you want the camera to be looking at the back of the target and 180 if you want the camera to be looking at the front of the target.
	public const float DEFAULT_Y_ROTATION_OFFSET = 0;
	float yRotationOffset = DEFAULT_Y_ROTATION_OFFSET;

	CinemachineVirtualCamera cmvc;

	PlayerController playerController;
	PlayerControl playerControl;
	PlayerAI playerAI;

	// Use this for initialization
	void Awake ()
 	{
		//If we are in multiplayer but not the owner of this component, disable it.
		if( GameManager.Instance.isMultiplayer() && !isAllowed() ) this.enabled = false;

		mainCamera = Camera.main.transform;
		cameraTarget = transform; //Set the player as the camera target by default
		playerController = GetComponent<PlayerController>();
		playerControl = GetComponent<PlayerControl>();
		playerAI = GetComponent<PlayerAI>(); //Null for everyone except bots

		if( isAllowed() )
		{
			cmvc = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
			cmvc.m_Follow = transform;
			cmvc.m_LookAt = transform;
		}
	}
	
	//When moving a player using rigid bodies, you need to use FixedUpdate or else you will have jitter.
	void FixedUpdate ()
	{
		if( cameraState == CameraState.Normal )
		{
			if( !isCameraLocked && cmvc == null )
			{
				positionCamera ();
			}
			if (shake_intensity > 0)
			{
				mainCamera.position = mainCamera.position + Random.insideUnitSphere * shake_intensity;
				mainCamera.rotation = new Quaternion(
					mainCamera.rotation.x + Random.Range (-shake_intensity,shake_intensity) * .2f,
					mainCamera.rotation.y + Random.Range (-shake_intensity,shake_intensity) * .2f,
					mainCamera.rotation.z + Random.Range (-shake_intensity,shake_intensity) * .2f,
					mainCamera.rotation.w + Random.Range (-shake_intensity,shake_intensity) * .2f);
				shake_intensity -= shake_decay;
			}
		}
	}

	public void setCameraTarget( Transform newTarget, bool instantTransition, float yRotationOffset )
	{
		if( !isAllowed() ) return;
		cameraTarget = newTarget;
		this.yRotationOffset = yRotationOffset;
		if( instantTransition )
		{
	
			// Calculate the current rotation angles
			float wantedRotationAngle = cameraTarget.eulerAngles.y + yRotationOffset;
			float wantedHeight = cameraTarget.position.y + height;

			// Convert the angle into a rotation
			Quaternion currentRotation = Quaternion.Euler (0, wantedRotationAngle, 0);
			mainCamera.rotation = currentRotation;
			// Set the position of the camera on the x-z plane to:
			// distance meters behind the target
			mainCamera.position = cameraTarget.position;
			mainCamera.position = currentRotation * Vector3.forward * distance;
			
			// Set the height of the camera
			mainCamera.position = new Vector3( mainCamera.position.x, wantedHeight, mainCamera.position.z );
			
			// Always look at the target
			mainCamera.LookAt (cameraTarget);
			
			//Tilt the camera down
			mainCamera.rotation = Quaternion.Euler( cameraXrotation, mainCamera.eulerAngles.y, mainCamera.eulerAngles.z );
		}
	}

	public void resetCameraTarget()
	{
		if( !isAllowed() ) return;
		setCameraTarget( transform, true, yRotationOffset );
	}

	public void setCameraParameters( float cameraXrotation, float distance, float height, float yRotationOffset )
	{
		if( !isAllowed() ) return;
		this.cameraXrotation = cameraXrotation;
		this.distance = distance;
		this.height = height;
		this.yRotationOffset = yRotationOffset;
	}
	
	public void resetCameraParameters()
	{
		if( !isAllowed() ) return;
		cameraXrotation = DEFAULT_CAMERA_X_ROT;
		//mainCamera.rotation = Quaternion.Euler( cameraXrotation, mainCamera.eulerAngles.y, mainCamera.eulerAngles.z );
		mainCamera.GetComponent<Camera>().fieldOfView = DEFAULT_MAIN_CAMERA_FOV;
		distance = DEFAULT_DISTANCE;
		height = DEFAULT_HEIGHT;
		yRotationOffset = DEFAULT_Y_ROTATION_OFFSET;
	}

	public void resetCutsceneCameraParameters()
	{
		if( !isAllowed() ) return;
		cutsceneCamera.localPosition = new Vector3( 0, 3.5f, 6f ); //Remember the cutscene camera is a child of the hero
		cutsceneCamera.localRotation = Quaternion.Euler( 17f,180f, 0 );
		cutsceneCamera.GetComponent<Camera>().fieldOfView = DEFAULT_CUTSCENE_FOV;
	}

	public void activateMainCamera()
	{
		if( !isAllowed() ) return;
		//Give back control to the main camera
		cameraState = CameraState.Normal;
		cutsceneCamera.gameObject.SetActive( false );
	}

	private void positionCamera ()
	{
		// Calculate the current rotation angles
		float wantedRotationAngle = cameraTarget.eulerAngles.y + yRotationOffset;
		float wantedHeight = cameraTarget.position.y + height;
			
		float currentRotationAngle = mainCamera.eulerAngles.y;
		float currentHeight = mainCamera.position.y;
		
		// Damp the rotation around the y-axis
		currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
	
		// Damp the height
		currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);
	
		// Convert the angle into a rotation
		Quaternion currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);
		
		// Set the position of the camera on the x-z plane to:
		// distance meters behind the target
		mainCamera.position = cameraTarget.position;
		mainCamera.position -= currentRotation * Vector3.forward * distance;
	
		// Set the height of the camera
		mainCamera.position = new Vector3( mainCamera.position.x, currentHeight, mainCamera.position.z );
		
		// Always look at the target
		mainCamera.LookAt (cameraTarget);
		
		//Tilt the camera down
		mainCamera.rotation = Quaternion.Euler( cameraXrotation, mainCamera.eulerAngles.y, mainCamera.eulerAngles.z );
	}
	
	public void positionCameraNow ()
	{
		if( !isAllowed() ) return;
		// Calculate the current rotation angles
		float wantedRotationAngle = cameraTarget.eulerAngles.y + yRotationOffset;
		float wantedHeight = cameraTarget.position.y + height;
		
		float currentRotationAngle = mainCamera.eulerAngles.y;
		float currentHeight = mainCamera.position.y;
		
		// Damp the rotation around the y-axis
		currentRotationAngle = wantedRotationAngle;
		
		// Damp the height
		currentHeight = wantedHeight;
		
		// Convert the angle into a rotation
		Quaternion currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);
		
		// Set the position of the camera on the x-z plane to:
		// distance meters behind the target
		mainCamera.position = cameraTarget.position;
		mainCamera.position -= currentRotation * Vector3.forward * distance;
		
		// Set the height of the camera
		mainCamera.position = new Vector3( mainCamera.position.x, currentHeight, mainCamera.position.z );
		
		// Always look at the target
		mainCamera.LookAt (cameraTarget);
		
		//Tilt the camera down
		mainCamera.rotation = Quaternion.Euler( cameraXrotation, mainCamera.eulerAngles.y, mainCamera.eulerAngles.z );
	}

	//A rotation offset of 0 means the camera is looking at the back. A rotation offset of 180 means
	//the camera is looking at the front.
	public void setRotationOffset( float angle )
	{
		if( !isAllowed() ) return;
		yRotationOffset = angle;
	}

	public void resetRotationOffset()
	{
		if( !isAllowed() ) return;
		yRotationOffset = DEFAULT_Y_ROTATION_OFFSET;
	}

	public void lockCamera( bool isCameraLocked )
	{
		if( !isAllowed() ) return;
		this.isCameraLocked = isCameraLocked;
	}
	
	public void Shake()
	{
		if( !isAllowed() ) return;
		shake_intensity = 0.12f;
		shake_decay = 0.006f;
	}

	public void playCutscene( CutsceneType type )
	{
		if( cutsceneCamera == null || !isAllowed() ) return;
		if( playerController != null ) playerController.enablePlayerControl( false );
		if( playerControl != null ) playerControl.enablePlayerControl( false );
		cutsceneCamera.gameObject.SetActive( true );
		cameraState = CameraState.Cutscene;
		
		if( type == CutsceneType.Start )
		{
			StartCoroutine( activateCutscene( 2f, mainCamera.position.y, DEFAULT_CAMERA_X_ROT, DEFAULT_MAIN_CAMERA_FOV, true, 180f ) );
		}
		else if( type == CutsceneType.Troll )
		{
			StartCoroutine( activateCutscene( 2f, mainCamera.position.y, 14f, 46f, false, 167f ) );
		}
		else if( type == CutsceneType.CullisGate )
		{
			StartCoroutine( activateCutscene( 2f, mainCamera.position.y, 14f, 68f, false, 144f ) );
		}
		else if( type == CutsceneType.Checkpoint )
		{
			cameraState = CameraState.Normal;
			cameraXrotation = DEFAULT_CAMERA_X_ROT;
			cutsceneCamera.gameObject.SetActive( false );
		}
		else if( type == CutsceneType.SummonSkeletons )
		{
			Vector3 aBitCloserPosition = transform.TransformPoint(new Vector3( 0 ,0 , -1.74f ));
			aBitCloserPosition.y = mainCamera.transform.position.y;
			StartCoroutine( activateCutscene2( 2f, aBitCloserPosition, 12f, DEFAULT_MAIN_CAMERA_FOV ) );
		}
		else if( type == CutsceneType.OpeningSequence )
		{
			cutsceneCamera.localPosition = new Vector3( 0, -40.3f, 50f );
			cutsceneCamera.rotation = Quaternion.Euler( 330f,180f, 3f );
			cutsceneCamera.GetComponent<Camera>().fieldOfView = 54.9f;
			cutsceneCamera.transform.parent = null;
		}
		else if( type == CutsceneType.MagicGate )
		{
			cutsceneCamera.localPosition = new Vector3( 0, 2.6f, 147f );
			cutsceneCamera.rotation = Quaternion.Euler( 352.6f,-180f, 1.5f );
			cutsceneCamera.GetComponent<Camera>().fieldOfView = 54.9f;
		}
		else if( type == CutsceneType.Ziplining )
		{
			cutsceneCamera.localPosition = new Vector3( 0.3f, 2.9f, -10f );
			cutsceneCamera.rotation = Quaternion.Euler( 17f,2.8f, 0f );
		}
	}
	
	IEnumerator activateCutscene( float duration, float endYpos, float endXrot, float endFOV, bool giveBackControl, float rotateAmount )
	{
		//Time
		float startTime = Time.time;
		float elapsedTime = 0;
		
		//Y position
		float startYpos = cutsceneCamera.position.y;
		
		//X rotation
		float startXrot = cutsceneCamera.eulerAngles.x;
		
		//FOV
		float startFOV = cutsceneCamera.GetComponent<Camera>().fieldOfView;	
		
		//Steps
		float step = 0f; //non-smoothed
		float rate = 1f/duration; //amount to increase non-smooth step by
		float smoothStep = 0f; //smooth step this time
		float lastStep = 0f; //smooth step last time

		
		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;
			
			//Percentage of time completed 
			float fracJourney = elapsedTime / duration;
			if( fracJourney > 1f) fracJourney = 1f;
		
			//Position
			float yPos = Mathf.Lerp( startYpos, endYpos, fracJourney );
			//Don't touch the X and Z positions has they are changed by RotateAround
			cutsceneCamera.transform.position = new Vector3 ( cutsceneCamera.transform.position.x, yPos, cutsceneCamera.transform.position.z );
			
			//FOV
			cutsceneCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp( startFOV, endFOV, fracJourney );
			
			//Order of rotations is ZXY
			//We are not rotating in Z
			
			//X rotation
			float xRot = Mathf.LerpAngle( startXrot, endXrot, fracJourney );
			cutsceneCamera.transform.eulerAngles = new Vector3 ( xRot, cutsceneCamera.eulerAngles.y, cutsceneCamera.eulerAngles.z );
			
			//Rotate around player in Y
			step += Time.deltaTime * rate; //increase the step
        	smoothStep = Mathf.SmoothStep(0f, 1f, step); //get the smooth step
			cutsceneCamera.RotateAround( transform.position, Vector3.up, rotateAmount * (smoothStep - lastStep) );
			lastStep = smoothStep; //store the smooth step
       		
			yield return new WaitForFixedUpdate();  

		}
	    //finish any left-over
	    if(step > 1f)
		{
			cutsceneCamera.RotateAround( transform.position, Vector3.up, rotateAmount * (1f - lastStep));
		}
		yield return new WaitForEndOfFrame();   
		
		if( giveBackControl )
		{
			//Give back control to the main camera
			resetCameraParameters();
			mainCamera.transform.position = new Vector3( cutsceneCamera.position.x, cutsceneCamera.transform.position.y, cutsceneCamera.position.z );
			mainCamera.transform.rotation = Quaternion.Euler( cutsceneCamera.eulerAngles.x, cutsceneCamera.eulerAngles.y, cutsceneCamera.eulerAngles.z );
			cameraState = CameraState.Normal;
			cutsceneCamera.gameObject.SetActive( false );
			if( playerController != null ) playerController.enablePlayerControl( true );
			if( playerControl != null ) playerControl.enablePlayerControl( true );
		}

		Debug.Log("SimpleCamera-activateCutscene finished: " + cutsceneCamera.GetComponent<Camera>().fieldOfView + " " + giveBackControl );
		
	}

	IEnumerator activateCutscene2( float duration, Vector3 endPos, float endXrot, float endFOV )
	{

		//Position cut-scene camera exactly where main camera is for a seamless transition
		cutsceneCamera.transform.parent = null;
		cutsceneCamera.position = new Vector3( mainCamera.position.x, mainCamera.transform.position.y, mainCamera.position.z );
		cutsceneCamera.rotation = Quaternion.Euler( mainCamera.eulerAngles.x, mainCamera.eulerAngles.y, mainCamera.eulerAngles.z );
		cutsceneCamera.GetComponent<Camera>().fieldOfView = mainCamera.GetComponent<Camera>().fieldOfView;

		//Time
		float startTime = Time.time;
		float elapsedTime = 0;
		
		//Position
		Vector3 startPos = cutsceneCamera.position;
		
		//X rotation
		float startXrot = cutsceneCamera.eulerAngles.x;
		
		//FOV
		float startFOV = cutsceneCamera.GetComponent<Camera>().fieldOfView;	

		
		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;
			
			//Percentage of time completed 
			float fracJourney = elapsedTime / duration;
			if( fracJourney > 1f) fracJourney = 1f;
		
			//Position
			cutsceneCamera.transform.position = Vector3.Lerp( startPos, endPos, fracJourney );
			
			//FOV
			cutsceneCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp( startFOV, endFOV, fracJourney );
			
			//X rotation
			float xRot = Mathf.LerpAngle( startXrot, endXrot, fracJourney );
			cutsceneCamera.transform.eulerAngles = new Vector3 ( xRot, cutsceneCamera.eulerAngles.y, cutsceneCamera.eulerAngles.z );

			yield return new WaitForFixedUpdate();
		}
	}

	public void	reactivateMaincamera()
	{
		if( !isAllowed() ) return;
		//Give back control to the main camera
		resetCameraParameters();
		mainCamera.transform.position = new Vector3( cutsceneCamera.position.x, cutsceneCamera.transform.position.y, cutsceneCamera.position.z );
		mainCamera.transform.rotation = Quaternion.Euler( cutsceneCamera.eulerAngles.x, cutsceneCamera.eulerAngles.y, cutsceneCamera.eulerAngles.z );
		cameraState = CameraState.Normal;
		cutsceneCamera.gameObject.SetActive( false );
		cutsceneCamera.transform.parent = transform;
	}

	/// <summary>
	/// Used by public methods in PlayerCamera to verify if the call should be allowed. We only want the local real player to affect the camera.
	/// </summary>
	/// <returns><c>true</c>, if allowed, <c>false</c> otherwise.</returns>
	bool isAllowed()
	{
		if( GameManager.Instance.isMultiplayer() )
		{
			return this.photonView.isMine && playerAI == null;
		}
		else
		{
			return true;
		}
	}
}
