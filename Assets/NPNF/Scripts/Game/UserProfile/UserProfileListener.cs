using UnityEngine;
using NPNF.Core;
using NPNF.Core.CollectionModule;
using System.Collections.Generic;

namespace NPNF.Game
{
    public abstract class UserProfileListener : NPNFGameListener
    {
        [SerializeField] protected UserProfile profile;
        
        protected virtual void OnEnable()
        {
            profile.OnInitialized += InitializeHandler;
            profile.OnInitializeError += InitializeErrorHandler;
            profile.OnCustomRemoved += CustomRemovedHandler;
            profile.OnCustomUpdated += CustomUpdatedHandler;
            profile.OnEntitlementAdded += EntitlementAddedHandler;
            profile.OnEntitlementRemoved += EntitlementRemovedHandler;
            profile.OnEntitlementUpdated += EntitlementUpdatedHandler;
        }

        protected virtual void OnDisable()
        {
            profile.OnInitialized -= InitializeHandler;
            profile.OnInitializeError -= InitializeErrorHandler;
            profile.OnCustomRemoved -= CustomRemovedHandler;
            profile.OnCustomUpdated -= CustomUpdatedHandler;
            profile.OnEntitlementAdded -= EntitlementAddedHandler;
            profile.OnEntitlementRemoved -= EntitlementRemovedHandler;
            profile.OnEntitlementUpdated -= EntitlementUpdatedHandler;
        }

        protected abstract void CustomRemovedHandler (string key, object value);
        protected abstract void CustomUpdatedHandler (string key, object value);
        protected abstract void EntitlementAddedHandler (List<Entitlement> entitlements);
        protected abstract void EntitlementRemovedHandler (List<Entitlement> entitlements);
        protected abstract void EntitlementUpdatedHandler (List<Entitlement> entitlements);            
    }
}
