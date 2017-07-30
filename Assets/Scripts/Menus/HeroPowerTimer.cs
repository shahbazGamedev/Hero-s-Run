using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public enum HeroPowerType
{
		SENTRY = 1,
		SHRINK = 2,
		CLOAK = 3,
		SHOCKWAVE = 4
}

public class HeroPowerTimer : MonoBehaviour {

	[SerializeField] GameObject holder;
	[SerializeField] Image radialImage;
	[SerializeField] Image radialImageMask;
	[SerializeField] Text nameText;
	[SerializeField] Text centerText;
	[SerializeField] ParticleSystem shockwaveEffect;

	bool ready = false;
	Transform localPlayer;
	HeroPowerType currentHeroPower = HeroPowerType.SHOCKWAVE;
	string powerName;
	float chargeTime = 5f;

	void OnEnable()
	{
		PlayerRace.crossedFinishLine += CrossedFinishLine;
		HUDMultiplayer.startRunningEvent += StartRunningEvent;
	}

	void OnDisable()
	{
		PlayerRace.crossedFinishLine -= CrossedFinishLine;
		HUDMultiplayer.startRunningEvent -= StartRunningEvent;
	}

	void StartRunningEvent()
	{
		//get a reference to the local player
		for(int i = 0; i < PlayerRace.players.Count; i++ )
		{
			if( PlayerRace.players[i].GetComponent<PhotonView>().isMine && PlayerRace.players[i].GetComponent<PlayerAI>() == null )
			{
				localPlayer = PlayerRace.players[i].transform;
				print("HeroPowerTimer-StartRunningEvent " + localPlayer.name  );
				break;
			}
		}
		HeroManager.HeroCharacter hero = HeroManager.Instance.getHeroCharacter( GameManager.Instance.playerProfile.selectedHeroIndex );
		if( hero.sex == Sex.MALE )
		{
			currentHeroPower = HeroPowerType.SHOCKWAVE;
			powerName = "Shockwave";
		}
		else
		{
			currentHeroPower = HeroPowerType.SENTRY;
			powerName = "Sentry";
		}
		startAnimation ( powerName, chargeTime, Color.gray );
	}

	void CrossedFinishLine( Transform player, int officialRacePosition, bool isBot )
	{
		if( !isBot )
		{
			holder.SetActive( false );
		}
	}

	public void startAnimation ( string name, float duration, Color color )
	{
		holder.SetActive( true );
		nameText.text = name;
		StartCoroutine( animate( duration ) );
		radialImage.color = color;
	}
	
	IEnumerator animate( float duration  )
	{
		float startTime = Time.time;
		float elapsedTime = 0;

		float fromValue = 0;
	
		while ( elapsedTime <= duration )
		{
			elapsedTime = Time.time - startTime;

			radialImageMask.fillAmount =  Mathf.Lerp( fromValue, 1f, elapsedTime/duration );
			centerText.text = string.Format( "{0:P0}", radialImageMask.fillAmount );
			yield return new WaitForEndOfFrame();  
	    }
		centerText.text = "Ready";
		ready = true;
	}

	public void activatePower ()
	{
		if( ready )
		{
			ready = false;
			startAnimation ( powerName, chargeTime, Color.blue );
			activate();
		}
	}

	void activate()
	{
		switch( currentHeroPower )
		{
			case HeroPowerType.CLOAK: 
			break;

			case HeroPowerType.SENTRY: 
				sentry();
			break;

			case HeroPowerType.SHOCKWAVE: 
				shockwave();
			break;

			case HeroPowerType.SHRINK: 
			break;
		}

	}

	void sentry()
	{
		object[] data = new object[4];
		data[0] = localPlayer.GetComponent<PhotonView>().viewID;
		data[1] = 25f;
		data[2] = 150f;
		data[3] = 0.005f;

		PhotonNetwork.InstantiateSceneObject( "Sentry", localPlayer.position, localPlayer.rotation, 0, data );
	}

	void shockwave()
	{
		//To add a dramatic effect, make all of the objects that have the Movable layer and which have a rigidbody move because of the shockwave.
		Collider[] movableHitColliders = Physics.OverlapSphere( localPlayer.position, 100f, MaskHandler.getMaskOnlyMovable() );
		for( int i = 0; i < movableHitColliders.Length; i++ )
		{
			if( movableHitColliders[i].attachedRigidbody != null )
			{
				movableHitColliders[i].attachedRigidbody.AddExplosionForce( 1500, localPlayer.TransformPoint( new Vector3( 0, 0.5f, 1f ) ), 50f, 1500f );
			}
		}

		//Add a particle system for the shockwave in front of the player
		GameObject go = GameObject.Instantiate( shockwaveEffect.gameObject );
		go.transform.position = localPlayer.TransformPoint( new Vector3( 0, 0.25f, 1.5f ) );
		go.GetComponent<ParticleSystem>().Play();

		//Shake the camera
		localPlayer.GetComponent<PlayerCamera>().Shake();

		//Nearby players dies. Players that are further stumble.
		Collider[] hitColliders = Physics.OverlapSphere( localPlayer.position, 100f, MaskHandler.getMaskOnlyPlayer() );
		for( int i = 0; i < hitColliders.Length; i++ )
		{
			//Ignore the caster
			if( hitColliders[i].transform == localPlayer.transform ) continue;

			//Ignore players in the Idle or Dying state
			if( hitColliders[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Idle || hitColliders[i].GetComponent<PlayerControl>().getCharacterState() == PlayerCharacterState.Dying ) continue;

			float distance = Vector3.Distance( hitColliders[i].transform.position, localPlayer.position );
			if( distance < 25f )
			{
				//Player is near. Kill him.
				topplePlayer( hitColliders[i].transform );
			}
			else
			{
				//Player is a bit further. Make him stumble.
				hitColliders[i].GetComponent<PlayerControl>().stumble();
			}
		}
	}

	void topplePlayer( Transform potentialTarget )
	{
		if( getDotProduct( potentialTarget, localPlayer.position ) )
		{
			//Explosion is in front of player. He falls backward.
			potentialTarget.GetComponent<PlayerControl>().killPlayer( DeathType.Obstacle );
		}
		else
		{
			//Explosion is behind player. He falls forward.
			potentialTarget.GetComponent<PlayerControl>().killPlayer( DeathType.FallForward );
		}
	}

	bool getDotProduct( Transform player, Vector3 explosionLocation )
	{
		Vector3 forward = player.TransformDirection(Vector3.forward);
		Vector3 toOther = explosionLocation - player.position;
		if (Vector3.Dot(forward, toOther) < 0)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

}