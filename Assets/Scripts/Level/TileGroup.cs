using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ValidGameMode {
	Story = 1,
	Endless = 2,
	Any = 3
}

[System.Serializable]
public class TileGroup{

	[Header("Tile Group Parameters")]
	[Tooltip("The theme.")]
	public SegmentTheme theme = SegmentTheme.Forest;
	public string name;
	public string description;
	public ValidGameMode ValidGameMode = ValidGameMode.Any;
	//Other ideas: validDifficultyLevel, allowEnemies, requiresZeroRotation, hasZeroRotation
	public List<TileType> tileList = new List<TileType>();
	[Range(0,100)]
	public int frequency = 0;
}
