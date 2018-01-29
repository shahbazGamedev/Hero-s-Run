using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorManager : MonoBehaviour {

	public static SectorManager Instance;
	public const int MAX_SECTOR = 4;
	public const int DEFAULT_POINTS_DELTA = 299;
	[SerializeField] List<SectorData> sectorDataList = new List<SectorData>();

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
			/*print(" 0 getSectorByPoints 0:   " + getSectorByPoints( 0 ) );
			print(" 1 getSectorByPoints 1:   " + getSectorByPoints( 1 ) );
			print(" 1 getSectorByPoints 100:   " + getSectorByPoints( 100 ) );
			print(" 1 getSectorByPoints 400:   " + getSectorByPoints( 400 ) );
			print(" 2 getSectorByPoints 401:   " + getSectorByPoints( 401 ) );
			print(" 2 getSectorByPoints 500:   " + getSectorByPoints( 500 ) );
			print(" 2 getSectorByPoints 800:   " + getSectorByPoints( 800 ) );
			print(" 3 getSectorByPoints 801:   " + getSectorByPoints( 801 ) );
			print(" 3 getSectorByPoints 1100:   " + getSectorByPoints( 1100 ) );
			print(" 4 getSectorByPoints 1101:   " + getSectorByPoints( 1101 ) );
			print(" 4 getSectorByPoints 1400:   " + getSectorByPoints( 1400 ) );
			print(" 4 getSectorByPoints 1401:   " + getSectorByPoints( 1401 ) );
			print(" 4 getSectorByPoints 69000:   " + getSectorByPoints( 69000 ) );
			print(" *****" );
			print(" Error -1 getPointsRange -1:   " + getPointsRange( -1 ) );
			print(" 0 getPointsRange 0:   " + getPointsRange( 0 ) );
			print(" 1 getPointsRange 1:   " + getPointsRange( 1 ) );
			print(" 2 getPointsRange 2:   " + getPointsRange( 2 ) );
			print(" 3 getPointsRange 3:   " + getPointsRange( 3 ) );
			print(" 4 getPointsRange 4:   " + getPointsRange( 4 ) );
			print(" Error 5 getPointsRange 5:   " + getPointsRange( 5 ) );*/
		}
	}

	/// <summary>
	/// Returns the sector (between 0 and MAX_SECTOR) based on the number of competitive points.
	/// Returns 0 if the number of competitive points is 0.
	/// </summary>
	/// <returns>The sector based on the number of points.</returns>
	/// <param name="competitivePoints">Number of competitive points.</param>
	public int getSectorByPoints( int competitivePoints )
	{
		if( competitivePoints == 0 ) return 0;

		if( competitivePoints >= sectorDataList[MAX_SECTOR].pointsRequired ) return MAX_SECTOR;

		return sectorDataList.FindLastIndex( sector => sector.pointsRequired <= competitivePoints );
	}

	public int getPointsRange( int sector )
	{
		if( sector < 0 || sector > MAX_SECTOR )
		{
			Debug.LogError("PlayerProfile-the sector specified " + sector + " is incorrect. It needs to be between 0 and " + SectorManager.MAX_SECTOR.ToString() + ".");
			return -1;
		}
		else
		{
			if( sector == 0 ) return 0;
			if( sector == MAX_SECTOR ) return DEFAULT_POINTS_DELTA;

			return sectorDataList[sector+1].pointsRequired - 1 - sectorDataList[sector].pointsRequired;
		}
	}
	
	public int getPointsRequired( int sector )
	{
		if( sector < 0 || sector > MAX_SECTOR )
		{
			Debug.LogError("PlayerProfile-the sector specified " + sector + " is incorrect. It needs to be between 0 and " + SectorManager.MAX_SECTOR.ToString() + ".");
			return -1;
		}
		else
		{
			return sectorDataList[sector].pointsRequired;
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
			return sectorDataList[sector].sectorImage;
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
			return sectorDataList[sector].sectorColor;
		}
	}

	public int getSectorVictorySoftCurrency( int sector )
	{
		if( sector < 0 || sector > MAX_SECTOR )
		{
			Debug.LogError("PlayerProfile-the sector specified " + sector + " is incorrect. It needs to be between 0 and " + SectorManager.MAX_SECTOR.ToString() + ".");
			return 0;
		}
		else
		{
			return sectorDataList[sector].victorySoftCurrency;
		}
	}

	[System.Serializable]
	public class SectorData
	{
		public int pointsRequired; 
		public int victorySoftCurrency; 
		public Sprite sectorImage;
		public Color sectorColor;
	}

}
