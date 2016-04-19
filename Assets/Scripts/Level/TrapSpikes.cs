using UnityEngine;
using System.Collections;

public class TrapSpikes : MonoBehaviour {

	Animation animation;
	Animator animator;
	public AnimationClip open;
	PlayerController playerController;
	PowerUpManager powerUpManager;

	// Use this for initialization
	void Start ()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		playerController = player.gameObject.GetComponent<PlayerController>();
		GameObject powerUpManagerObject = GameObject.FindGameObjectWithTag("PowerUpManager");
		powerUpManager = powerUpManagerObject.GetComponent<PowerUpManager>();

		animation = GetComponent<Animation>();
		if( animation == null )
		{
			animator = GetComponent<Animator>();
		}
	}
	
	
	void playAnim( AnimationClip clip )
	{
		if( animation != null )
		{
			animation[clip.name].wrapMode = WrapMode.Once;
			animation[clip.name].speed = 1.4f;
			animation.Play(clip.name);
		}
	}
	
	void OnEnable()
	{
		GameManager.gameStateEvent += GameStateChange;
	}
	
	void OnDisable()
	{
		GameManager.gameStateEvent -= GameStateChange;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if( other.name == "Hero" && !CheatManager.Instance.getIsInvincible() )
		{
			//Is the player protected by a Shield Power Up?
			if( PowerUpManager.isThisPowerUpActive( PowerUpType.Shield ) )
			{
				//This Power Up only works one time, so deactivate it
				powerUpManager.deactivatePowerUp( PowerUpType.Shield, false );
			}
			else
			{
				triggerTrap();
			}
		}
	}

	void triggerTrap()
	{
		print ("Player trigger spike trap "  );
		GetComponent<AudioSource>().Play();
		playerController.managePlayerDeath(DeathType.Trap);
		if( animation != null )
		{
			playAnim(open);
		}
		else
		{
			animator.SetTrigger("open");
		}
	}
	
	void GameStateChange( GameState newState )
	{
		if( newState == GameState.Paused )
		{
			if( animation != null )
			{
				animation.enabled = false;
			}
			else
			{
				animator.enabled = false;
			}
			
		}
		else if( newState == GameState.Normal )
		{
			if( animation != null )
			{
				animation.enabled = true;
			}
			else
			{
				animator.enabled = true;
			}
		}
	}
}
