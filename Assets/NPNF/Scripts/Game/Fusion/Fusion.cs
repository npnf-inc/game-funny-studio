using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using NPNF.Core;
using NPNF.Core.CollectionModule;
using NPNF.Core.UserModule;
using NPNF.Core.FusionModule;
using NPNF.Core.FormulaModule;

namespace NPNF.Game
{
    /**
     * Fusion Resource Game Component
     * 
     * Integrates one Fusion configured on the Portal into your game.
     * 
     * This component should be attached to your Fusion game object to load the game object with
     * the specified formula/
     * 
     * Events are also provided for listening after Fuse is done.
     */

    [Serializable]
    public class Fusion : NPNFGame<NPNF.Core.FusionModule.Fusion>
    {
        // Name of the formula
        [SerializeField] public string formulaName;
        [SerializeField] private FusionTrigger[] triggers;

        #region Properties
        public string Name
        {
            get
            {
                return formulaName;
            }
        }
        #endregion

        #region Events
        /**
         * Fires when Fuse is done
         * @param fusion Fusion Resource Component
         * @param trigger If applicable, the Fusion Trigger Component used to perform the Fuse operation
         * @param FormulaResult contains the resource obtained after a fusion is applied
         */
        public event Action<Fusion, FusionTrigger, FormulaResult> OnFuseDone;
        /**
         * Fires when an error occurs while appling the formula
         * @param fusion Fusion Resource Component
         * @param trigger If applicable, the Fusion Trigger Component used to perform the Fuse operation
         * @param error The error which occurred during Fuse
         */
        public event Action<Fusion, FusionTrigger, NPNFError> OnFuseError;
        #endregion

        protected override void OnPlatformInit()
        {
            base.OnPlatformInit();

            // Will be replaced by formula GetByName in 2.0
            NPNF.Core.FusionModule.Fusion.GetAll(true, (List<NPNF.Core.FusionModule.Fusion> formulaList, NPNFError error) =>
            {
                if (error != null)
                {
                    Debug.LogError("Formula Get fail: " + error.Messages [0]);
                    TriggerInitializeError(error);
                } else
                {
                    Model = NPNF.Core.FusionModule.Fusion.GetCached(formulaName);
                    if (Model != null)
                    {
                        TriggerInitialized();
                    } else
                    {
                        TriggerInitializeError(NPNFError.GetGameError(NPNFError.GameCode.UNKNOWN_FUSION));
                    }
                }
            });
        }

        protected override void Start()
        {
            base.Start();
            if (triggers != null)
            {
                foreach (FusionTrigger trigger in triggers)
                {
                    trigger.OnTrigger += TriggerHandler;
                }
            }
        }

        private void TriggerHandler(FusionTrigger trigger)
        {
            if (User.IsCurrentProfileExist())
            {
                Fuse(trigger.priceName, trigger, trigger.fusionExecutions);
            }
        }

        /**
         * Apply a formula with a specific set of entitlements to be consumed
         * @param executions a specific set of entitlements to be consumed
         */
        public void Fuse(string priceName, List<FuseExecutions> executions = null)
        {
            Fuse(priceName, null, executions);
        }

        private void Fuse(string priceName, FusionTrigger trigger, List<FuseExecutions> executions)
        {
            if (!IsInitialized)
            {
                NPNFError error = NPNFError.GetGameError(NPNFError.GameCode.FUSION_NOT_INITIALIZED);
                if (OnFuseError != null)
                {
                    OnFuseError(this, trigger, error);
                }
                FinishTrigger(trigger, error);
                return;
            }

            User.CurrentProfile.Fusion.Fuse(Model.Name, priceName, executions, 1, (FormulaResult result, NPNFError error) =>
                                            {
                if (error == null)
                {
                    if (OnFuseDone != null)
                    {
                        OnFuseDone(this, trigger, result);
                    }
                } else
                {
                    if (OnFuseError != null)
                    {
                        OnFuseError(this, trigger, error);
                    }
                }
                FinishTrigger(trigger, error);
            });
        }

        private void FinishTrigger(FusionTrigger trigger, NPNFError error)
        {
            if (trigger != null)
            {
                trigger.FinishTriggeredAction(this, trigger, error);
            }
        }
    }
}