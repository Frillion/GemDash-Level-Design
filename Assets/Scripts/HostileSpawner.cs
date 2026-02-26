using System;
using System.Collections.Generic;
using System.Linq;
using GemDash.Utils;
using UnityEngine;
using UnityEngine.Serialization;

public class HostileSpawner : SingletonMonoBehaviour<HostileSpawner> 
{
    [SerializeField] private Hostile prefab;
    [SerializeField] private List<Transform> spawnPoints;
    private readonly List<Hostile> _activeHostiles = new();
    private ObjectPool<Hostile> _hostilePool;

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
        PoolManager.Instance.Remove(prefab.name);
        if (createNew)
        {
            _hostilePool = new ObjectPool<Hostile>().CreateObjectPool(prefab);
        }
    }

    public void ResetMobs()
    {
        _activeHostiles.ForEach(hostile => hostile.Despawn());
        _activeHostiles.Clear();
        spawnPoints.ForEach(point => _activeHostiles.Add(_hostilePool.Spawn(point.position)));
    }

    public void Despawn(Hostile hostile)
    {
        _activeHostiles.Remove(hostile);
        hostile.Despawn();
    }
}
