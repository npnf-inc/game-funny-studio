using UnityEngine;
using NPNF.Core;
using NPNF.Core.CollectionModule;
using NPNF.Core.FormulaModule;

namespace NPNF.Game
{
    /**
     * Fusion Listener Game Component
     * 
     * Subclass this abstract class to handle all events of the
     * Fusion Resource Game Component.
     * 
     * All abstract handler methods need to be implemented
     */
    public abstract class FusionListener : NPNFGameListener
    {
        [SerializeField] protected Fusion fusion;

        /**
         * Handlers are attached to all Fusion events
         */
        protected virtual void OnEnable()
        {
            fusion.OnInitialized += InitializeHandler;
            fusion.OnInitializeError += InitializeErrorHandler;
            fusion.OnFuseDone += FuseDoneHandler;
            fusion.OnFuseError += FuseErrorHandler;
        }

        /**
         * Handlers are detached from all Fusion events
         */
        protected virtual void OnDisable()
        {
            fusion.OnInitialized -= InitializeHandler;
            fusion.OnInitializeError -= InitializeErrorHandler;
            fusion.OnFuseDone -= FuseDoneHandler;
            fusion.OnFuseError -= FuseErrorHandler;
        }

        protected abstract void FuseDoneHandler(Fusion fusion, FusionTrigger trigger, FormulaResult results);
        protected abstract void FuseErrorHandler(Fusion fusion, FusionTrigger trigger, NPNFError error);
    }
}
