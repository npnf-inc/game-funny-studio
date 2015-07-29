using UnityEngine;
using NPNF.Core;
using NPNF.Core.CollectionModule;
using System.Collections.Generic;

namespace NPNF.Game
{
    /**
     * Gacha Listener Game Component
     * 
     * Subclass this abstract class to handle all events of the
     * Gacha Resource Game Component.
     * 
     * All abstract handler methods need to be implemented
     */
    public abstract class GachaListener : NPNFGameListener
    {
        [SerializeField] protected Gacha gacha;

        /**
         * Handlers are attached to all Gacha events
         */
        protected virtual void OnEnable()
        {
            gacha.OnInitialized += InitializeHandler;
            gacha.OnInitializeError += InitializeErrorHandler;
            gacha.OnPlayDone += PlayDoneHandler;
            gacha.OnPlayError += PlayErrorHandler;
        }

        /**
         * Handlers are detached from all Gacha events
         */
        protected virtual void OnDisable()
        {
            gacha.OnInitialized -= InitializeHandler;
            gacha.OnInitializeError -= InitializeErrorHandler;
            gacha.OnPlayDone -= PlayDoneHandler;
            gacha.OnPlayError -= PlayErrorHandler;
        }

        protected abstract void PlayDoneHandler(Gacha gacha, GachaTrigger trigger, List<Entitlement> results);
        protected abstract void PlayErrorHandler(Gacha gacha, GachaTrigger trigger, NPNFError error);
    }
}
