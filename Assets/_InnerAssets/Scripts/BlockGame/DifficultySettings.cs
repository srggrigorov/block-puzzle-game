using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(
    fileName = nameof(DifficultySettings),
    menuName = "Settings/" + nameof(DifficultySettings))]
public class DifficultySettings : ModuleSettings
{
    [FormerlySerializedAs("difficultyStatsList")]
    [SerializeField]
    private List<DifficultyStats> _difficultyStatsList;

    private DifficultyStats? GetDifficultyStats(int currentScore)
    {
        var stats =  _difficultyStatsList.FirstOrDefault(x => x.Score == currentScore);
        return stats.CellsCountRandomWeightedValues != null ? stats : null;
    }

    public bool TryGetDifficultyStats(int currentScore)
    {
        return TryGetDifficultyStats(currentScore, out _);
    }

    public bool TryGetDifficultyStats(int currentScore, out DifficultyStats? difficultyStats)
    {
        difficultyStats = GetDifficultyStats(currentScore);
        return difficultyStats != null;
    }
}
