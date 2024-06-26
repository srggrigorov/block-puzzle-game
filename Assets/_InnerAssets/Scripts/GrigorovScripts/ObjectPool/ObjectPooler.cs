using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;
using Object = UnityEngine.Object;

public class ObjectPooler : IDisposable, IInitializable
{
    private readonly ObjectPoolerSettings _settings;
    private readonly DiContainer _container;
    private readonly Dictionary<string, ObjectPoolWithObjects> _objectPools;
    private readonly Dictionary<int, CreatedPooledObject> _createdObjects;

    public ObjectPooler(DiContainer container, AssetsManager assetsManager)
    {
        _container = container;
        _settings = assetsManager.GetModuleSettings<ObjectPoolerSettings>();
        _objectPools = new Dictionary<string, ObjectPoolWithObjects>();
        _createdObjects = new Dictionary<int, CreatedPooledObject>();
    }

    public void RegisterPrefabs()
    {
        foreach (var poolData in _settings.PoolsDataList)
        {
            RegisterPrefabInternal(poolData.Prefab, poolData.PrewarmCount, poolData.AutoDespawn,
                poolData.AutoDespawnDelaySec);
        }
    }

    private void RegisterPrefabInternal(MonoBehaviour prefab, int prewarmCount, bool autoDespawn,
        float autoDespawnDelaySec)
    {
        MonoBehaviour CreateFunc()
        {
            var createdObject = (MonoBehaviour)
                _container.InstantiatePrefabForComponent(
                    prefab.GetType(), prefab, Array.Empty<object>(),
                    GameObjectCreationParameters.Default);

            _objectPools[prefab.name].ObjectsInPool.Add(createdObject);
            _createdObjects[createdObject.GetHashCode()] = new CreatedPooledObject(
                prefab,
                autoDespawn,
                (int)(autoDespawnDelaySec * 1000), this);
            if (createdObject is IPoolableOverride poolableOverride)
            {
                poolableOverride.OnCreate();
            }

            return createdObject;
        }

        void ActionOnGet(MonoBehaviour pooledObject)
        {
            CreatedPooledObject createdPooledObject = _createdObjects[pooledObject.GetHashCode()];

            if (pooledObject is IPoolableOverride poolableOverride)
            {
                poolableOverride.OnGet();
            }
            else
            {
                pooledObject.gameObject.SetActive(true);
            }

            createdPooledObject.IsSpawned = true;
        }

        void ActionOnRelease(MonoBehaviour pooledObject)
        {

            if (pooledObject is IPoolableOverride poolableOverride)
            {
                poolableOverride.OnRelease();
            }
            else
            {
                pooledObject.transform.SetParent(null);
                pooledObject.gameObject.SetActive(false);
            }
        }

        void ActionOnDestroy(MonoBehaviour pooledObject)
        {
            int pooledObjectHashCode = pooledObject.GetHashCode();
            _createdObjects[pooledObjectHashCode].CancellationTokenSource?.Cancel();
            _createdObjects[pooledObjectHashCode].CancellationTokenSource?.Dispose();
            _createdObjects.Remove(pooledObjectHashCode);
            if (pooledObject is IPoolableOverride poolableOverride)
            {
                poolableOverride.OnDelete();
            }
            else
            {
                Object.Destroy(pooledObject.gameObject);
            }
        }

        _objectPools[prefab.name] = new ObjectPoolWithObjects
        {
            ObjectPool = new ObjectPool<MonoBehaviour>(
                CreateFunc, ActionOnGet, ActionOnRelease,
                ActionOnDestroy, defaultCapacity: prewarmCount),
        };

        var prewarmGameObjects = new List<MonoBehaviour>();
        for (var i = 0; i < prewarmCount; i++)
        {
            prewarmGameObjects.Add(_objectPools[prefab.name].ObjectPool.Get());
        }

        foreach (var prewarmObject in prewarmGameObjects)
        {
            _objectPools[prefab.name].ObjectPool.Release(prewarmObject);
        }
    }

    public MonoBehaviour Spawn(MonoBehaviour prefab)
    {
        if (!_objectPools.ContainsKey(prefab.name))
        {
            Debug.LogError($"No Object Pool with name {prefab.name}!");
            return null;
        }

        var spawnedObject = _objectPools[prefab.name].ObjectPool.Get();
        CreatedPooledObject createdPooledObject = _createdObjects[spawnedObject.GetHashCode()];
        if (createdPooledObject.AutoDespawn)
        {
            createdPooledObject.StartAutoDespawn(spawnedObject).Forget();
        }
        createdPooledObject.IsSpawned = true;
        return spawnedObject;
    }

    public MonoBehaviour Spawn(MonoBehaviour prefab, Vector3 position, Quaternion rotation,
        Transform parent = null)
    {
        var spawnedObject = Spawn(prefab);
        if (spawnedObject == null)
        {
            return null;
        }

        var spawnedObjectTransform = spawnedObject.transform;
        spawnedObjectTransform.SetPositionAndRotation(position, rotation);
        spawnedObjectTransform.SetParent(parent);

        return spawnedObject;
    }

    public void Despawn(MonoBehaviour prefabInstance)
    {
        var hashCode = prefabInstance.GetHashCode();

        if (!IsSpawned(hashCode))
        {
            Debug.LogWarning($"{prefabInstance.gameObject.name} is not spawned!");
            return;
        }

        CreatedPooledObject createdPooledObject = _createdObjects[hashCode];
        createdPooledObject.CancellationTokenSource?.Cancel();
        createdPooledObject.CancellationTokenSource?.Dispose();
        _objectPools[_createdObjects[hashCode].Prefab.name].ObjectPool.Release(prefabInstance);
        createdPooledObject.IsSpawned = false;
    }

    public bool IsSpawned(MonoBehaviour prefabInstance) => IsSpawned(prefabInstance.GetHashCode());

    private bool IsSpawned(int hashCode)
        => _createdObjects.TryGetValue(hashCode, out var pooledObject) && pooledObject.IsSpawned;

    public void Dispose()
    {
        _objectPools.Clear();
        _createdObjects.Clear();
        foreach (var spawnedObject in _createdObjects)
        {
            spawnedObject.Value.CancellationTokenSource?.Dispose();
        }
    }

    public void DespawnAllObjects(MonoBehaviour objectPrefab)
    {
        foreach (var objectInPool in _objectPools[objectPrefab.name].ObjectsInPool)
        {
            if (IsSpawned(objectInPool))
            {
                Despawn(objectInPool);
            }
        }
    }

    public void DespawnAllObjects(string prefabName)
    {
        foreach (var objectInPool in _objectPools[prefabName].ObjectsInPool)
        {
            if (IsSpawned(objectInPool))
            {
                Despawn(objectInPool);
            }
        }
    }

    public void DespawnAllObjects()
    {
        foreach (var pair in _objectPools)
        {
            DespawnAllObjects(pair.Key);
        }
    }
    
    public void Initialize()
    {
        RegisterPrefabs();
    }
}

[Serializable]
public class ObjectPoolWithObjects
{
    public ObjectPool<MonoBehaviour> ObjectPool;
    public HashSet<MonoBehaviour> ObjectsInPool = new HashSet<MonoBehaviour>();

}
