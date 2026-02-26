using System.Collections.Generic;
using Clash.Extensions;
using UnityEngine;

namespace GemDash.Utils
{
    [DefaultExecutionOrder(-1000)]
    public class PoolManager :  SingletonMonoBehaviour<PoolManager>
    {
        public Dictionary<string, Transform> PoolTransforms = new();
        public GenericDictionary PoolDict = new();

        public void Add(string key, object pool, Transform container)
        {
            PoolTransforms.Add(key, container);
            PoolDict.Add(key, pool);
        }

        public void Remove(string key)
        {
            if (PoolTransforms.TryGetValue(key, out Transform trans))
                trans.Clear();

            PoolDict.Remove(key);
            PoolTransforms.Remove(key);
        }

        public new void OnDestroy()
        {
            PoolTransforms.Clear(); 
            PoolDict.Dict.Clear();
            transform.Clear();
           
            base.OnDestroy();
        }
    }

    public class GenericDictionary
    {
        public readonly Dictionary<string, object> Dict = new();

        public void Add<T>(string key, T value) where T : class
        {
            Dict.Add(key, value);
        }

        public T Get<T>(string key) where T : class
        {
            return Dict[key] as T;
        }

        public void Remove(string key)
        {
            Dict.Remove(key);
        }
    }
}