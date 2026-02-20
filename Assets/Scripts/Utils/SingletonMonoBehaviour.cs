using System;
using UnityEngine;

namespace GemDash.Utils
{
    [DefaultExecutionOrder(-1000)]
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T: SingletonMonoBehaviour<T> 
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("Instance of " + typeof(T) + " missing");
                }
                return _instance;
            }
        }

        protected void Awake()
        {
            if (_instance != null)
            {
                Debug.LogWarning("Removing Instance of " + typeof(T) + " make sure to have one instance"); 
                Destroy(this); 
            }

            _instance = this as T;
            Debug.Log("Assigning Instance of " + typeof(T));
        }

        public void OnDestroy()
        {
            _instance = null;
        }
    }
}
