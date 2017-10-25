using UnityEngine;
using System.Collections;

public class TreeController : MonoBehaviour {
	

	enum TreeState {
		Sleeping = 0,
		Awake = 1,
	}

	//Components
	Animation treeAnimation;

	bool isLeftOfRoad = true;

	public ParticleSystem fallingLeaves;

	TreeState treeState = TreeState.Sleeping;

	public AudioClip wakeUp1;
	public AudioClip wakeUp2;

	Transform player;
	PlayerController playerController;
	bool hasAttackedPlayer = false;
	public bool appearsOnRandomRoadSide = true;

	//The tree can be on either side of the road (50/50).
	//If we are on the right side, we also need to move a normal tree to the other side to make room.
	public Transform treeToMove;

	void Awake()
	{
		//Get a copy of the components
		treeAnimation = GetComponent<Animation>();

		if( appearsOnRandomRoadSide )
		{
			//Decide if we are on the left or the right side.
			if( Random.value < 0.5f )
			{
				isLeftOfRoad = true;
				//Trees are already correcly positioned, so do nothing
			}
			else
			{
				//Evil tree will be on the right side of the road
				isLeftOfRoad = false;
				//Move evil tree to the right side of the road and normal tree to the left side of the road
				transform.localPosition 	= new Vector3( -transform.localPosition.x, transform.localPosition.y, transform.localPosition.z );
				if( treeToMove != null ) treeToMove.localPosition = new Vector3( -transform.localPosition.x, transform.localPosition.y, transform.localPosition.z );
				//We want the leaves particle system to shift to the left side of the tree.
				fallingLeaves.transform.localPosition = new Vector3( -fallingLeaves.transform.localPosition.x, fallingLeaves.transform.localPosition.y, fallingLeaves.transform.localPosition.z );
			}
		}
	}

	void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = player.GetComponent<PlayerController>();
	}

	//Called by the TriggerTree script
	public void wakeUp()
	{
		//Do not play the wake up animation if the tree is already awake because it will not blend well
		if( treeState == TreeState.Sleeping )
		{
			//Play one of two wake-up sounds
			AudioClip soundToPlay;
			if( Random.value < 0.5f )
			{
				soundToPlay = wakeUp1;
			}
			else
			{
				soundToPlay = wakeUp2;
			}
			GetComponent<AudioSource>().clip = soundToPlay;
			GetComponent<AudioSource>().Play();

			if( isLeftOfRoad )
			{
				treeAnimation.Play ("WakeUpLeft");
				treeAnimation.CrossFadeQueued("AwakeLookLeft", 0.2f, QueueMode.CompleteOthers);
			}
			else
			{
				treeAnimation.Play("WakeUpRight");
				treeAnimation.CrossFadeQueued("AwakeLookRight", 0.2f, QueueMode.CompleteOthers);
			}
			treeState = TreeState.Awake;
		}
	}

	//Note: the attack works well for player speeds up to 24 m/sec. Above that, the tree attack always misses because the player goes by too quickly.
	void attackPlayer()
	{
		hasAttackedPlayer = true;
		if( fallingLeaves != null ) fallingLeaves.Play();
		if( isLeftOfRoad )
		{
			//Attack low
			treeAnimation.CrossFade("HitLowLeft", 0.2f);
			Debug.Log ("tree attackPlayer LOW LEFT"  );
		}
		else
		{
			//Attack low
			treeAnimation.CrossFade("HitLowRight", 0.2f);
			Debug.Log ("tree attackPlayer LOW RIGHT"  );
		}
	}

	void Update()
	{
		if( !hasAttackedPlayer )
		{
			float distance = Vector3.Distance(player.position,transform.position);

			float attackDistance = 0.81f * playerController.getSpeed();
			if( distance < attackDistance )
			{
				attackPlayer();
			}
		}
	}

	void OnEnable()
	{
		PlayerController.playerStateChanged += PlayerStateChange;
		//Reset values
		treeState = TreeState.Sleeping;
		hasAttackedPlayer = false;
		treeAnimation.Stop();
	}
	
	void OnDisable()
	{
		PlayerController.playerStateChanged -= PlayerStateChange;
	}
	
	void PlayerStateChange( PlayerCharacterState newState )
	{
		if( newState == PlayerCharacterState.Dying )
		{
			if( treeState == TreeState.Sleeping )
			{
				if( isLeftOfRoad )
				{
					//Play victory animation, but wake up tree first
					treeAnimation.Play("WakeUpLeft");
					treeState = TreeState.Awake;
					treeAnimation.CrossFadeQueued("VictoryLeft", 0.33f);
					treeAnimation.PlayQueued("AwakeLookLeft", QueueMode.CompleteOthers);
					Debug.Log ("tree victory  left sleeping"  );
				}
				else
				{
					//Play victory animation, but wake up tree first
					treeAnimation.Play("WakeUpRight");
					treeState = TreeState.Awake;
					treeAnimation.CrossFadeQueued("VictoryRight", 0.33f);
					treeAnimation.PlayQueued("AwakeLookRight", QueueMode.CompleteOthers);
					Debug.Log ("tree victory  RIGHT sleeping"  );
				}
			}
			else
			{
				if( isLeftOfRoad )
				{
					//Victory animation
					treeAnimation.PlayQueued("VictoryLeft", QueueMode.CompleteOthers);
					treeAnimation.PlayQueued("AwakeLookLeft", QueueMode.CompleteOthers);
					Debug.Log ("tree victory  left awake"  );
				}
				else
				{
					//Victory animation
					treeAnimation.PlayQueued("VictoryRight", QueueMode.CompleteOthers);
					treeAnimation.PlayQueued("AwakeLookRight", QueueMode.CompleteOthers);
					Debug.Log ("tree victory  RIGHT awake"  );
				}
			}
		}
	}

}
