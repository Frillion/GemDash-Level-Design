using System;
using UnityEngine;

namespace GemDash.Utils
{
    public class Spawnable : MonoBehaviour,IPoolable
    {
        public IObjectPool Pool { get; set; }
        
        public Action OnSpawn;
        public Action OnDespawn;

        [HideInInspector] public bool Spawned;

        public virtual void Spawn()
        {
            if (Spawned)
            {
                if(!isActiveAndEnabled)
                    gameObject.SetActive(true);
                
                return;
            }

            Spawned = true;
            
            gameObject.SetActive(true);
            
            OnSpawn?.Invoke();
        }

        public virtual void Despawn()
        {
            if (!this)
                return;

            if (!Spawned && Pool != null)
                return;

            Spawned = false;
            
            gameObject.SetActive(false);
            
            if(Pool != null)
                Pool.Recycle(this, true);
            else
                Destroy(gameObject);
            
            OnDespawn?.Invoke();
        }
    }
}