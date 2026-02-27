using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GemDash.Utils;
using UnityEngine;
using UnityEngine.Serialization;

public class HostileSpawner : SingletonMonoBehaviour<HostileSpawner> 
{
    [SerializeField] private Hostile gemPrefab;
    [SerializeField] private Hostile staticGemPrefab;
    [SerializeField] private LifeTimeHostile lifetimeGemPrefab;
    
    [SerializeField] private List<Transform> movingGemSpawnPoints;
    [SerializeField] private List<Transform> staticGemSpawnPoints;
    [SerializeField] private List<Transform> continuousSpawners;
    
    private readonly List<Hostile> _activeHostiles = new();
    
    private ObjectPool<Hostile> _movingGemPool;
    private ObjectPool<Hostile> _staticGemPool;
    private ObjectPool<LifeTimeHostile> _lifetimeGemPool;


    private CancellationTokenSource _spawnTokenSource;

    [SerializeField] private float spawnDelay;
    private float _lastSpawn;
    

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (_spawnTokenSource != null)
        {
            _spawnTokenSource.Cancel();
            _spawnTokenSource.Dispose();
        }

        _spawnTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy(),
            CancellationToken.None);

        ResetPools();
        ResetMobs();
        SpawnLoop(_spawnTokenSource.Token).Forget();
    }

    private void ResetPools(bool createNew = true)
    {
        PoolManager.Instance.Remove(gemPrefab.name);
        PoolManager.Instance.Remove(staticGemPrefab.name);
        PoolManager.Instance.Remove(lifetimeGemPrefab.name);
        
        if (!createNew) return;
        
        _movingGemPool = new ObjectPool<Hostile>().CreateObjectPool(gemPrefab);
        _staticGemPool = new ObjectPool<Hostile>().CreateObjectPool(staticGemPrefab);
        _lifetimeGemPool = new ObjectPool<LifeTimeHostile>().CreateObjectPool(lifetimeGemPrefab);
    }

    public void ResetMobs()
    {
        _activeHostiles.ForEach(hostile => hostile.Despawn());
        _activeHostiles.Clear();
        movingGemSpawnPoints.ForEach(point => _activeHostiles.Add(_movingGemPool.Spawn(point.position)));
        staticGemSpawnPoints.ForEach(point => _activeHostiles.Add(_staticGemPool.Spawn(point.position)));
    }

    private async UniTask SpawnLoop(CancellationToken spawnTokneSource)
    {
        while (!spawnTokneSource.IsCancellationRequested)
        {
            continuousSpawners.ForEach(point => _activeHostiles.Add(_lifetimeGemPool.Spawn(point.position, point.rotation)));
            await UniTask.WaitForSeconds(spawnDelay, cancellationToken: spawnTokneSource);
        }
    }

    public void Despawn(Hostile hostile)
    {
        _activeHostiles.Remove(hostile);
        hostile.Despawn();
    }
}
