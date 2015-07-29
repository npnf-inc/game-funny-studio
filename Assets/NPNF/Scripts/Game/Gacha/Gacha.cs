using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using NPNF.Core;
using NPNF.Core.UserModule;
using NPNF.Core.GachaModule;
using NPNF.Core.CollectionModule;
using NPNF.Core.FormulaModule;

namespace NPNF.Game
{
    /**
     * Gacha Resource Game Component
     * 
     * Integrates one Gacha configured on the Portal into your game.
     * 
     * This component should be attached to your Gacha game object to load the game object with
     * the specified price
     * 
     * Events are also provided for listening after Gacha is played.
     */
    [Serializable]
    public class Gacha: NPNFGame<NPNF.Core.GachaModule.Gacha>
    {
        // Name of the gacha
        [SerializeField] public string gachaName;

        // List of pricing trigger
        [SerializeField] private GachaTrigger[] triggers;

        #region Properties
        public string Name
        {
            get
            {
                return gachaName;
            }
        }

        public TimeSpan TimeTillStart
        {
            get
            {
                if (Model != null && Model.StartTime != null)
                {
                    return ((DateTime)Model.StartTime - DateTime.Now);
                } else
                {
                    return TimeSpan.Zero;
                }
            }
        }

        public TimeSpan TimeTillEnd
        {
            get
            {
                if (Model != null && Model.EndTime != null)
                {
                    return ((DateTime)Model.EndTime - DateTime.Now);
                } else
                {
                    return TimeSpan.Zero;
                }
            }
        }
        #endregion

        #region Events
        /**
         * Fires when Play is done
         * @param gacha Gacha Resource Component
         * @param trigger If applicable, the Gacha Trigger Component used to perform the Play operation
         * @param entitlements a list of entitlements obtained after playing the gacha
         */
        public event Action<Gacha, GachaTrigger, List<Entitlement>> OnPlayDone;
        /**
         * Fires when an error occurs while Playing this gacha
         * @param gacha Gacha Resource Component
         * @param trigger If applicable, the Gacha Trigger Component used to perform the Play operation
         * @param error The error which occurred during play
         */
        public event Action<Gacha, GachaTrigger, NPNFError> OnPlayError;
        #endregion

        protected override void OnPlatformInit()
        {
            base.OnPlatformInit();
            NPNF.Core.GachaModule.Gacha.Get(gachaName, true, (NPNF.Core.GachaModule.Gacha gacha, NPNFError error) => {  
                if (error != null)
                {
                    TriggerInitializeError(error);
                } else
                {
                    Model = NPNF.Core.GachaModule.Gacha.GetCached(gachaName);
                    if (Model != null)
                    {
                        TriggerInitialized();
                    } else
                    {
                        TriggerInitializeError(NPNFError.GetGameError(NPNFError.GameCode.UNKNOWN_GACHA));
                    }
                }
            });
        }

        protected override void Start()
        {
            base.Start();
            if (triggers != null)
            {
                foreach (GachaTrigger trigger in triggers)
                {
                    trigger.OnTrigger += TriggerHandler;
                }
            }
        }
        
        private void TriggerHandler(GachaTrigger trigger)
        {
            if (User.IsCurrentProfileExist())
            {
                GachaTrigger gachaTrigger = (GachaTrigger)trigger;
                Play(gachaTrigger.priceName, gachaTrigger.purchaseCount, gachaTrigger);
            }
        }

        /**
         * Play a gacha with a pre-defined pricing model
         * @param priceName Name of the price to apply
         * @param purchaseCount (optional) Number of gacha plays to make (defaults to 1)
         */
        public void Play(string priceName, int purchaseCount = 1)
        {
            Play(priceName, purchaseCount, null);
        }

        private void Play(string priceName, int purchaseCount, GachaTrigger trigger)
        {
            if (!IsInitialized)
            {
                NPNFError error = NPNFError.GetGameError(NPNFError.GameCode.GACHA_NOT_INITIALIZED);
                if (OnPlayError != null)
                {
                    OnPlayError(this, trigger, error);
                }
                FinishTrigger(trigger, error);
                return;
            }

            User.CurrentProfile.Gacha.Play(Name, priceName, purchaseCount, (FormulaResult result, NPNFError error) => {
                if (error == null)
                {
                    List<Entitlement> entitlements = new List<Entitlement>();
                    
                    foreach (var entitlement in result.AddedEntitlements)
                    {
                        entitlements.Add(User.CurrentProfile.Entitlements.GetCached(entitlement.Id));
                    }

                    if (OnPlayDone != null)
                    {
                        OnPlayDone(this, trigger, entitlements);
                    }
                } else
                {
                    if (OnPlayError != null)
                    {
                        OnPlayError(this, trigger, error);
                    }
                }
                FinishTrigger(trigger, error);
            });
        }

        private void FinishTrigger(GachaTrigger trigger, NPNFError error)
        {
            if (trigger != null)
            {
                trigger.FinishTriggeredAction(this, trigger, error);
            }
        }
    }
}

