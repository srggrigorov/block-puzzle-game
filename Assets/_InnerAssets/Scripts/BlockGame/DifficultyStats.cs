using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DifficultyStats
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
