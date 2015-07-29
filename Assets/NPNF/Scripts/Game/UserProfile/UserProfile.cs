using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NPNF.Core;
using NPNF.Core.UserModule;
using NPNF.Core.CollectionModule;

namespace NPNF.Game
{
    public class UserProfile : NPNFGame<NPNF.Core.UserModule.UserProfile>
    {
        #region Properties
        public string ProfileId { get; private set; }
        public string UserId { get; private set; }
        #endregion

        #region Events
        /**
         * An event for the addition to or updating of the custom field.
         * The custom updated event will not fire if the custom value type is a non-primitive type 
         * like a Collection or some Mutable class, and the object is edited directly.
         */
        public event Action<string, object> OnCustomUpdated;

        /**
         * An event for the removal from the custom field.
         */
        public event Action<string, object> OnCustomRemoved;

        /**
         * An event for the addition of an entitlement.
         */
        public event Action<List<Entitlement>> OnEntitlementAdded;

        /**
         * An event for the removal of an entitlement.
         */
        public event Action<List<Entitlement>> OnEntitlementRemoved;

        /**
         * An event for the updating of an entitlement.
         */
        public event Action<List<Entitlement>> OnEntitlementUpdated;
        #endregion

        protected override void OnUserLoaded(User user)
        {
            base.OnUserLoaded(user);
            if (Model != null)
            {
                Model.OnCustomUpdated -= CustomUpdatedHandler;
                Model.OnCustomRemoved -= CustomRemovedHandler;
                Model.Entitlements.OnAdded -= OnEntitlementAdded;
                Model.Entitlements.OnRemoved -= OnEntitlementRemoved;
                Model.Entitlements.OnUpdated -= OnEntitlementUpdated;
            }
            Model = user.Profiles.Current;
            ProfileId = Model.Id;
            UserId = Model.UserId;
            AttachEventListeners();
            TriggerInitialized();
        }

        private void AttachEventListeners()
        {
            Model.OnCustomUpdated += CustomUpdatedHandler;
            Model.OnCustomRemoved += CustomRemovedHandler;
            Model.Entitlements.OnAdded += OnEntitlementAdded;
            Model.Entitlements.OnRemoved += OnEntitlementRemoved;
            Model.Entitlements.OnUpdated += OnEntitlementUpdated;
        }

        private void CustomUpdatedHandler(string key, object value)
        {
            if (OnCustomUpdated != null)
                OnCustomUpdated(key, value);
        }

        private void CustomRemovedHandler(string key, object value)
        {
            if (OnCustomRemoved != null)
                OnCustomRemoved(key, value);
        }

        private void EntitlementAddedHandler(List<Entitlement> entitlements)
        {
            if (OnEntitlementAdded != null)
                OnEntitlementAdded(entitlements);
        }

        private void EntitlementRemovedHandler(List<Entitlement> entitlements)
        {
            if (OnEntitlementRemoved != null)
                OnEntitlementRemoved(entitlements);
        }

        private void EntitlementUpdatedHandler(List<Entitlement> entitlements)
        {
            if (OnEntitlementUpdated != null)
                OnEntitlementUpdated(entitlements);
        }
    }
}
