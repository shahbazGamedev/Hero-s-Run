using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LockstepActionType {

	NONE = 0,
	RACE_START = 1,
	RESURRECT = 2,
	UNPAUSE = 3,
	ATTACH_TO_ZIPLINE = 4,
	DETACH_FROM_ZIPLINE = 5,
	JUMP_PAD = 6,
	TELEPORTER = 7,
	CARD = 8
}

public class LockstepManager : MonoBehaviour {

	[SerializeField] double lockedStepTime = 0.0625; //Note: 62.5 ms is the value used in Hero of the Storm
	double lastActionTime = 0;
	public static LockstepManager Instance;
	public Queue<LockstepAction> lockstepCurrentActionQueue = new Queue<LockstepAction>();
	public Queue<LockstepAction> lockstepNextActionQueue = new Queue<LockstepAction>();

	void Awake ()
	{
		if( Instance == null ) Instance = this;
	}

	public void initiateFirstAction ( double timeWhenFirstActionShouldBeProcessed )
	{
		Invoke("firstAction", (float)timeWhenFirstActionShouldBeProcessed );
	}
	
	void firstAction()
	{
		addActionToQueue( new LockstepAction( LockstepActionType.RACE_START ) );
		lastActionTime = PhotonNetwork.time;
	}

	// Update is called once per frame
	void Update ()
	{
		if( GameManager.Instance.isOnlinePlayMode() )
		{
			if( PhotonNetwork.time - lastActionTime >= lockedStepTime)
			{
				lastActionTime = PhotonNetwork.time;
				processQueue();
			}
		}
		else
		{
			//When playing offline, might as well process actions immediately
			processQueue();
		}
	}

	void processQueue()
	{
		while( lockstepCurrentActionQueue.Count > 0 )
		{
			process( lockstepCurrentActionQueue.Dequeue() );
		}
		//Now add the actions from the next action queue
		while( lockstepNextActionQueue.Count > 0 )
		{
			lockstepCurrentActionQueue.Enqueue( lockstepNextActionQueue.Dequeue() );
		}
	}

	void process( LockstepAction lockstepAction )
	{
		switch( lockstepAction.type )
		{
			case LockstepActionType.RACE_START:
				HUDMultiplayer.hudMultiplayer.startCountdown();
			break;

			case LockstepActionType.RESURRECT:
				lockstepAction.actor.GetComponent<PlayerControl>().resurrectBegin();
			break;

			case LockstepActionType.UNPAUSE:
				lockstepAction.actor.GetComponent<PlayerControl>().pausePlayer( false );
			break;

			case LockstepActionType.ATTACH_TO_ZIPLINE:
				lockstepAction.actor.GetComponent<PlayerControl>().attachToZipline();
			break;

			case LockstepActionType.DETACH_FROM_ZIPLINE:
				lockstepAction.actor.GetComponent<PlayerControl>().detachFromZipline();
			break;

			case LockstepActionType.JUMP_PAD:
				lockstepAction.actor.GetComponent<PlayerControl>().enablePlayerMovement( true );
				lockstepAction.actor.GetComponent<PlayerControl>().doubleJump( lockstepAction.param1 );
			break;

			case LockstepActionType.TELEPORTER:
				//Simply move the player to the teleporter receiver.
				//See the Teleporter class for what happens when the receiver is triggered.
				//Note that MovePosition is not immediate. It takes effect at the next FixedUpdate() call.
				lockstepAction.actor.transform.GetComponent<Rigidbody>().MovePosition( lockstepAction.param3 );
			break;

			case LockstepActionType.CARD:
				if( lockstepAction.cardSpawnedObject != null ) lockstepAction.cardSpawnedObject.activateCard();
			break;
		}
	}

	public void addActionToQueue( LockstepAction lockstepAction )
	{
		lockstepNextActionQueue.Enqueue( lockstepAction );
		print("adding action to queue " + lockstepAction.type );
	}

	public class LockstepAction
	{
		public LockstepActionType type; 
		public GameObject actor;
		public CardName cardName;
		public float param1;
		public int param2;
		public Vector3 param3;
		public CardSpawnedObject cardSpawnedObject;

		public LockstepAction( LockstepActionType type, GameObject actor = null, CardName cardName = CardName.None, float param1 = 0 )
		{
			this.type = type;
			this.actor = actor;
			this.cardName = cardName;
			this.param1 = param1;
		}
	}

}
