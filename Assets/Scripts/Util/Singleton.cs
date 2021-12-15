using UnityEngine;

namespace Util
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        public static bool IsInitialized => Instance != null;

        protected virtual void Awake()
        {
            if (Instance == null)
                Instance = (T) this;
            else
                Debug.LogError("[Singleton] Trying to instantiate a second instance of a singleton class.");
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}