using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

[Serializable]
public class WeightedValue<T>
{
    [FormerlySerializedAs("Value")]
    public T value;
    [FormerlySerializedAs("Weight")]
    public float weight;

    public static T GetRandomValue(IEnumerable<WeightedValue<T>> weightedValueList)
    {
        return weightedValueList.Random(weightedValue => weightedValue.weight).value;
    }
}