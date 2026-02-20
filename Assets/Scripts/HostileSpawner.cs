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

    protected new void Awake()
    {
        base.Awake();
        _hostilePool = new ObjectPool<Hostile>().CreateObjectPool(prefab);
    }

    protected void Start()
    {
        spawnPoints.ForEach(point => _activeHostiles.Add(_hostilePool.Spawn(point.position)));
    }

    public void ResetMobs()
    {
        _activeHostiles.ForEach(hostile => hostile.Despawn());
        _activeHostiles.Clear();
        Start();
    }

    public void Despawn(Hostile hostile)
    {
        _activeHostiles.Remove(hostile);
        hostile.Despawn();
    }
}
