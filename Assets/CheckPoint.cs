using System;
using AGDDPlatformer;
using UnityEngine;
using UnityEngine.Serialization;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    private bool _checked;

    private void Awake()
    {
        _checked = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_checked)
            return;

        var player = other.TryGetComponent<PlayerController>(out var component);
        if (!player)
            return;
        
        component.SetResetPosition(respawnPoint.position);
        _checked = true;
    }
}
