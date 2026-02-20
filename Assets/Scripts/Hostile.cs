using System;
using AGDDPlatformer;
using GemDash.Utils;
using UnityEngine;

public class Hostile : Spawnable
{
   private Vector3 _originalPosition;

   private void Awake()
   {
      _originalPosition = transform.position;
   }

   private void OnTriggerEnter2D(Collider2D other)
   {
      var player = other.GetComponent<PlayerController>();
      if (!player) return;

      if (player.isDashing)
      {
         player.ResetDash();
         HostileSpawner.Instance.Despawn(this);
      }
      else
      {
         player.ResetPlayer();
         HostileSpawner.Instance.ResetMobs();
      }
   }
}
