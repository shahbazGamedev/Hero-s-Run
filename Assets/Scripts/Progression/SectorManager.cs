using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorManager : MonoBehaviour {

	public static SectorManager Instance;
	public const int MAX_SECTOR = 4;
	public const int DEFAULT_TROPHY_DELTA = 299;
	[SerializeField] List<int> trophiesRequiredPerSector = new List<int>(MAX_SECTOR);
	[SerializeField] Sprite genericSectorImage;

	void Awake ()
	{
		if(Instance)
		{
			DestroyImmediate(gameObject);
		}
		else
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
			print(" 0 getSectorByTrophies 0:   " + getSectorByTrophies( 0 ) );
			print(" 1 getSectorByTrophies 1:   " + getSectorByTrophies( 1 ) );
			print(" 1 getSectorByTrophies 100:   " + getSectorByTrophies( 100 ) );
			print(" 1 getSectorByTrophies 400:   " + getSectorByTrophies( 400 ) );
			print(" 2 getSectorByTrophies 401:   " + getSectorByTrophies( 401 ) );
			print(" 2 getSectorByTrophies 500:   " + getSectorByTrophies( 500 ) );
			print(" 2 getSectorByTrophies 800:   " + getSectorByTrophies( 800 ) );
			print(" 3 getSectorByTrophies 801:   " + getSectorByTrophies( 801 ) );
			print(" 3 getSectorByTrophies 1100:   " + getSectorByTrophies( 1100 ) );
			print(" 4 getSectorByTrophies 1101:   " + getSectorByTrophies( 1101 ) );
			print(" 4 getSectorByTrophies 1400:   " + getSectorByTrophies( 1400 ) );
			print(" 4 getSectorByTrophies 1401:   " + getSectorByTrophies( 1401 ) );
			print(" 4 getSectorByTrophies 69000:   " + getSectorByTrophies( 69000 ) );
			print(" *****" );
			print(" Error -1 getTrophyRange -1:   " + getTrophyRange( -1 ) );
			print(" 0 getTrophyRange 0:   " + getTrophyRange( 0 ) );
			print(" 1 getTrophyRange 1:   " + getTrophyRange( 1 ) );
			print(" 2 getTrophyRange 2:   " + getTrophyRange( 2 ) );
			print(" 3 getTrophyRange 3:   " + getTrophyRange( 3 ) );
			print(" 4 getTrophyRange 4:   " + getTrophyRange( 4 ) );
			print(" Error 5 getTrophyRange 5:   " + getTrophyRange( 5 ) );
		}
	}

	/// <summary>
	/// Returns the sector (between 0 and MAX_SECTOR) based on the number of trophies.
	/// Returns 0 if the number of trophies is 0.
	/// </summary>
	/// <returns>The sector based on the number of trophies.</returns>
	/// <param name="numberOfTrophies">Number of trophies.</param>
	public int getSectorByTrophies( int numberOfTrophies )
	{
		if( numberOfTrophies == 0 ) return 0;

		if( numberOfTrophies >= trophiesRequiredPerSector[MAX_SECTOR] ) return MAX_SECTOR;

		return trophiesRequiredPerSector.FindLastIndex( sector => sector <= numberOfTrophies );
	}

	public int getTrophyRange( int sector )
	{
		if( sector < 0 || sector > MAX_SECTOR )
		{
			Debug.LogError("PlayerProfile-the sector specified " + sector + " is incorrect. It needs to be between 0 and " + SectorManager.MAX_SECTOR.ToString() + ".");
			return -1;
		}
		else
		{
			if( sector == 0 ) return 0;
			if( sector == MAX_SECTOR ) return DEFAULT_TROPHY_DELTA;

			return trophiesRequiredPerSector[sector+1] - 1 - trophiesRequiredPerSector[sector];
		}
	}
	
	public int getTrophiesRequired( int sector )
	{
		if( sector < 0 || sector > MAX_SECTOR )
		{
			Debug.LogError("PlayerProfile-the sector specified " + sector + " is incorrect. It needs to be between 0 and " + SectorManager.MAX_SECTOR.ToString() + ".");
			return -1;
		}
		else
		{
			return trophiesRequiredPerSector[sector];
		}
	}

	public Sprite getSectorImage( int sector )
	{
		if( sector < 0 || sector > MAX_SECTOR )
		{
			Debug.LogError("PlayerProfile-the sector specified " + sector + " is incorrect. It needs to be between 0 and " + SectorManager.MAX_SECTOR.ToString() + ".");
			return null;
		}
		else
		{
			return genericSectorImage;
		}
	}

	public Color getSectorColor( int sector )
	{
		if( sector < 0 || sector > MAX_SECTOR )
		{
			Debug.LogError("PlayerProfile-the sector specified " + sector + " is incorrect. It needs to be between 0 and " + SectorManager.MAX_SECTOR.ToString() + ".");
			return Color.red;
		}
		else
		{
			return Color.blue;
		}
	}

}
