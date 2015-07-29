using UnityEngine;
using System.Collections;
using System;
using NPNF;
using NPNF.Core.UserModule;
using NPNF.Core;

namespace NPNF.Game
{
    public abstract class NPNFGame<TModel> : MonoBehaviour
    {
        private bool mTriggeredPlatformInit;

        #region Properties
        /**
         * The Core API object used to load the data for this Resource object
         */
        public TModel Model { get; protected set; }

        /**
         * Indicates whether the data for this Resource object is completely loaded or not
         */
        public bool IsInitialized { get; private set; }
        #endregion

        #region Events
        #pragma warning disable 67
        /**
         * Triggers when initialization is completed for this Resource
         */
        public event System.Action OnInitialized;

        /**
         * Triggers when there is an error occurring during initialization
         */
        public event Action<NPNFError> OnInitializeError;
        #pragma warning restore 67
        #endregion

        protected virtual void OnEnable()
        {
            NPNFMain.OnInitComplete += InitCompleteHandler;
            User.OnLoaded += OnUserLoaded;
        }

        protected virtual void OnDisable()
        {
            NPNFMain.OnInitComplete -= InitCompleteHandler;
            User.OnLoaded -= OnUserLoaded;
        }

        protected virtual void Start()
        {
            if (!mTriggeredPlatformInit && Platform.IsInitialized)
            {
                OnPlatformInit();
                mTriggeredPlatformInit = true;
            }
            if (User.CurrentProfile != null)
            {
                OnUserLoaded(User.Current);
            }
        }

        private void InitCompleteHandler(NPNFError error)
        {
            if (!mTriggeredPlatformInit)
            {
                OnPlatformInit();
                mTriggeredPlatformInit = true;
            }
        }

        protected virtual void OnPlatformInit()
        {
        }

        protected virtual void OnUserLoaded(User user)
        {
        }

        protected void TriggerInitialized()
        {
            IsInitialized = true;
            if (OnInitialized != null)
            {
                OnInitialized();
            }
        }

        protected void TriggerInitializeError(NPNFError error)
        {
            IsInitialized = false;
            if (OnInitializeError != null)
            {
                OnInitializeError(error);
            }
        }
    }
}