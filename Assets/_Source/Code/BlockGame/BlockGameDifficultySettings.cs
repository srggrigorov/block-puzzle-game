using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dreamloft.Game.Minigames.BlockGame
{
	[CreateAssetMenu(
		fileName = "BlockGameDifficultySettings",
		menuName = "Data/Difficulty Settings")]
	public class BlockGameDifficultySettings : ScriptableObject
	{
		[SerializeField]
		private List<BlockGameDifficultyStats> difficultyStatsList;

		public BlockGameDifficultyStats GetDifficultyStats(int currentScore)
		{
			return difficultyStatsList.FirstOrDefault(x => x.Score == currentScore);
		}

		public bool TryGetDifficultyStats(int currentScore)
		{
			return TryGetDifficultyStats(currentScore, out _);
		}

		public bool TryGetDifficultyStats(int currentScore, out BlockGameDifficultyStats difficultyStats)
		{
			difficultyStats = GetDifficultyStats(currentScore);
			return difficultyStats != null;
		}
	}

	[Serializable]
	public class BlockGameDifficultyStats
	{
		[field: SerializeField]
		public int Score { get; private set; }
		[field: SerializeField]
		public List<WeightedValue<int>> CellsCountRandomWeightedValues { get; private set; }

		public int GetRandomShapeCellsCount()
		{
			return WeightedValue<int>.GetRandomValue(CellsCountRandomWeightedValues);
		}
	}
}