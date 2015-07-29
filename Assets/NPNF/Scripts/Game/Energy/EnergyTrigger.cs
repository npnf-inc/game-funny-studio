using NPNF.Game;
using NPNF.Core.UserModule;
using System;
using UnityEngine;
using NPNF.Core;

namespace NPNF.Game
{
    /**
     * Energy Trigger Game Component
     * 
     * Add this component to a game object to allow it to trigger an Energy action
     * This trigger needs to be added to the Energy Resource Component where
     * you would like to perform the action
     */
    [Serializable]
    public class EnergyTrigger : NPNFGameTrigger<EnergyTrigger, Energy>
    {
        /**
         * Energy-related Actions
         */
        [Serializable]
        public enum Action
        {
            Debit,
            Credit
        };

        /**
         * The selected action for this trigger
         */
        public Action action;

        /**
         * Energy amount to debit or credit
         */
        public int amount;

        protected override void Activate()
        {
            StartTriggerAction(this);
        }
    }
}
