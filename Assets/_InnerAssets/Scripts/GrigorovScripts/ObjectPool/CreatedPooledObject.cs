using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CreatedPooledObject
{
    public MonoBehaviour Prefab { get; }
    public bool AutoDespawn { get; }
    public int AutoDespawnDelayMs { get; }
    public CancellationTokenSource CancellationTokenSource { get; private set; }
    public bool IsSpawned;

    private ObjectPooler _objectPooler;

    public CreatedPooledObject(MonoBehaviour prefab, bool autoDespawn, int autoDespawnDelayMs, ObjectPooler objectPooler)
    {
        Prefab = prefab;
        AutoDespawn = autoDespawn;
        AutoDespawnDelayMs = autoDespawnDelayMs;
        _objectPooler = objectPooler;
    }


    public async UniTask StartAutoDespawn(MonoBehaviour pooledObject)
    {
        CancellationTokenSource = new CancellationTokenSource();

        try
        {
            await Task.Delay(AutoDespawnDelayMs, cancellationToken: CancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (Application.isPlaying)
        {
            _objectPooler.Despawn(pooledObject);
        }
    }
}
