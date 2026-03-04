using System;
using AGDDPlatformer;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private Transform _respawnPoint;
    private bool _checked;

    private void Awake()
    {
        _respawnPoint = GetComponentInChildren<Transform>();
        _checked = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_checked)
            return;

        var player = other.TryGetComponent<PlayerController>(out var component);
        if (!player)
            return;
        
        component.SetResetPosition(_respawnPoint.position);
    }
}
