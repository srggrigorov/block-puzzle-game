using System;
using System.Collections.Generic;

[Serializable]
public class WeightedValue<T>
{
    public T Value;
    public float Weight;

    public static T GetRandomValue(IEnumerable<WeightedValue<T>> weightedValueList)
    {
        return weightedValueList.Random(weightedValue => weightedValue.Weight).Value;
    }
}