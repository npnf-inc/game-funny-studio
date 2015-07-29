using System;
using UnityEngine;

namespace SKPlanet
{
    /// <summary>
    /// Mono behaviour singleton.
    /// </summary>
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
    {
        protected static T instance;

        /**
          Returns the instance of this singleton.
        */
        public static T Instance
        {
            get
            {
                if (UnityEngine.Object.ReferenceEquals(instance, null))
                {
                    instance = (T)FindObjectOfType(typeof(T));

                    if (UnityEngine.Object.ReferenceEquals(instance, null))
                    {
                        instance = (new GameObject(typeof(T).Name.ToString())).AddComponent<T>();
                    }
                }
                return instance;
            }
            set
            {
                if (!UnityEngine.Object.ReferenceEquals(instance, null))
                {
                    instance = value;
                }
            }
        }

        // Helper function to initialize the Singleton object and invoke the Init() function
        public void Load()
        {

        }

        // If no other monobehaviour request the instance in an awake function
        // executing before this one, no need to search the object.
        protected void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            // DontDestroyOnLoad(this.gameObject);
            instance.Init();
        }

        // This function is called when the instance is used the first time
        // Put all the initializations you need here, as you would do in Awake
        public abstract void Init();

        // Make sure the instance isn't referenced anymore when the user quit, just in case.
        private void OnApplicationQuit()
        {

        }
    }
}

