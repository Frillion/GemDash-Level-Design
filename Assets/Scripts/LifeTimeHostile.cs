using UnityEngine;
using UnityEngine.Serialization;

public class LifeTimeHostile : Hostile
{
    [SerializeField]private float ttl;
    private float _lifetime;

    protected override void Awake()
    {
        base.Awake();
        _lifetime = 0;
    }

    protected override void Update()
    {
        base.Update();
        _lifetime += Time.deltaTime;
        if(_lifetime >= ttl) HostileSpawner.Instance.Despawn(this);
    }

    public override void Despawn()
    {
        _lifetime = 0;
        base.Despawn();
    }
}
