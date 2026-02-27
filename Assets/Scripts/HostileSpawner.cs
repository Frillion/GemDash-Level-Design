using System;
using System.Collections.Generic;
using System.Linq;
using GemDash.Utils;
using UnityEngine;
using UnityEngine.Serialization;

public class HostileSpawner : SingletonMonoBehaviour<HostileSpawner> 
{
    [SerializeField] private Hostile gemPrefab;
    [SerializeField] private Hostile staticGemPrefab;
    [SerializeField] private List<Transform> movingGemSpawnPoints;
    [SerializeField] private List<Transform> staticGemSpawnPoints;
    private readonly List<Hostile> _activeHostiles = new();
    private ObjectPool<Hostile> _movingGemPool;
    private ObjectPool<Hostile> _staticGemPool;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        ResetPools();
        ResetMobs();
    }

    private void ResetPools(bool createNew = true)
    {
        PoolManager.Instance.Remove(gemPrefab.name);
        PoolManager.Instance.Remove(staticGemPrefab.name);
        
        if (!createNew) return;
        
        _movingGemPool = new ObjectPool<Hostile>().CreateObjectPool(gemPrefab);
        _staticGemPool = new ObjectPool<Hostile>().CreateObjectPool(staticGemPrefab);
    }

    public void ResetMobs()
    {
        _activeHostiles.ForEach(hostile => hostile.Despawn());
        _activeHostiles.Clear();
        movingGemSpawnPoints.ForEach(point => _activeHostiles.Add(_movingGemPool.Spawn(point.position)));
        staticGemSpawnPoints.ForEach(point => _activeHostiles.Add(_staticGemPool.Spawn(point.position)));
    }

    public void Despawn(Hostile hostile)
    {
        _activeHostiles.Remove(hostile);
        hostile.Despawn();
    }
}
