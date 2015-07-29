using UnityEngine;
using System.Collections;
using NPNF.Core;

namespace NPNF.Game
{
    /**
     * Energy Listener Game Component
     * 
     * Subclass this abstract class to handle all events of the
     * Energy Resource Game Component.
     * 
     * All abstract handler methods need to be implemented
     */
    public abstract class EnergyListener : NPNFGameListener
    {
        [SerializeField] protected Energy energy;

        /**
         * Handlers are attached to all Energy events
         */
        protected virtual void OnEnable()
        {
            energy.OnInitialized += InitializeHandler;
            energy.OnInitializeError += InitializeErrorHandler;
            energy.OnUpdated += UpdatedHandler;
            energy.OnCreditError += CreditErrorHandler;
            energy.OnDebitError += DebitErrorHandler;
        }

        /**
         * Handlers are detached from all Energy events
         */
        protected virtual void OnDisable()
        {
            energy.OnInitialized -= InitializeHandler;
            energy.OnInitializeError -= InitializeErrorHandler;
            energy.OnUpdated -= UpdatedHandler;
            energy.OnCreditError -= CreditErrorHandler;
            energy.OnDebitError -= DebitErrorHandler;
        }

        protected abstract void UpdatedHandler(int value, int previousValue);
        protected abstract void CreditErrorHandler(Energy energy, EnergyTrigger trigger, NPNFError error);
        protected abstract void DebitErrorHandler(Energy energy, EnergyTrigger trigger, NPNFError error);
    }
}
