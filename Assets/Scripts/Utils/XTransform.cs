using UnityEngine;

namespace Clash.Extensions
{
    public static class XTransform  
    {
        public static void Clear(this Transform trans)
        {
            if (!trans)
                return;

            foreach (var child in trans.GetComponentsInChildren<Transform>())
            {
                if (!child || child == trans || !child.gameObject)
                    return;
            
                if(Application.isPlaying)
                    Object.Destroy(child.gameObject);                
                else
                    Object.DestroyImmediate(child.gameObject);
            }
        }
    }
}