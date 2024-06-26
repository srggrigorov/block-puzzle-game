using System;
using UnityEngine;

[Serializable]
public class PoolData
{
    [field: SerializeField]
    [field: SerializeReference]
    public MonoBehaviour Prefab { get; private set; }
    [field: SerializeField]
    public int PrewarmCount { get; private set; }
    [field: SerializeField]
    public bool AutoDespawn { get; private set; }
    [field: SerializeField]
    public float AutoDespawnDelaySec { get; private set; }
}
