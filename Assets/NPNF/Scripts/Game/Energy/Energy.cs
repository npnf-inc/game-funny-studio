using UnityEngine;
using System.Collections;
using NPNF.Core.UserModule;
using System;
using NPNF.Core;
using NPNF.Core.EnergyModule;

namespace NPNF.Game
{
    /**
     * Energy Resource Game Component
     * 
     * Integrates one Energy configured on the Portal into your game.
     * This component should be attached to your Energy Bar game object to load the game object with
     * the specified Energy for the current UserProfile.
     * 
     * Events are also provided for listening to changes to the Energy's value.
     */
    [Serializable]
    public class Energy : NPNFGame<EnergyBank>
    {
        protected override void Start()
        {
            base.Start();

            if (triggers != null)
            {
                foreach (EnergyTrigger trigger in triggers)
                {
                    trigger.OnTrigger += TriggerHandler;
                }
            }
        }

        protected override void OnUserLoaded(User user)
        {
            base.OnUserLoaded(user);

            if (mIsInitializing)
            {
                mPendingUser = user;
                return;
            }
            mPendingUser = null;
            mIsInitializing = true;
            
            if (Model != null)
            {
                Model.RemoveTimeToRechargeNextUnitHandler(energyName, TimeUntilIncrementHandler);
                Model.RemoveValueUpdateHandler(energyName, ValueUpdateHandler);
                Model.RemoveMaxUnitReachedHandler(energyName, MaxUnitReachedHandler);
            }
            
            NPNF.Core.EnergyModule.Energy.Get(energyName, true, (NPNF.Core.EnergyModule.Energy getEnergy, NPNFError error) => {
                if (getEnergy == null)
                {
                    TriggerInitializeError(NPNFError.GetGameError(NPNFError.GameCode.UNKNOWN_ENERGY));
                    return;
                }
                OnEnergyLoaded(getEnergy, user.Profiles.Current.EnergyBank);
            });
        }

        
        protected void FinishTrigger(EnergyTrigger trigger, NPNFError error)
        {
            if (trigger != null)
            {
                trigger.FinishTriggeredAction(this, trigger, error);
            }
        }


        [SerializeField]
        private string energyName;

        // Empty String sets local notification off
        [SerializeField]
        private String localNotificationMessage;

        [SerializeField]
        private EnergyTrigger[] triggers;

        #region Properties
        /**
         * Current value of this Energy
         */
        public int Value { get; private set; }

        /**
         * Minimum possible value for this Energy
         */
        public int MinValue { get; private set; }

        /**
         * Maximum possible value for this Energy
         */
        public int MaxValue { get; private set; }

        /**
         * Flag to indicate whether this Energy is enabled or disabled
         */
        public bool Enabled { get; private set; }

        /**
         * Name of this Energy
         */
        public string Name
        {
            get
            {
                return energyName;
            }
        }

        /**
         * Value between 0f and 1.0f for the ratio between the current value and the maximum value
         */
        public float Ratio
        {
            get
            {
                if (MaxValue == 0)
                    return 0f;
                else
                    return (float)Value / (float)MaxValue;
            }
        }

        /**
         * Number of seconds until this Energy gains the next unit
         */
        public int SecondsTillIncrement
        {
            get
            {
                if (IsInitialized)
                    return (int)Model.GetTimeUntilIncrement(energyName);
                else
                    return -1;
            }
        }

        /**
         * Number of seconds until this Energy becomes reaches its maximum value
         */
        public int SecondsTillFull
        {
            get
            {
                if (IsInitialized)
                    return (int)Model.GetTimeUntilFullRecharge(energyName);
                else
                    return -1;
            }
        }

        /**
         * Total number of seconds to recharge a single unit of Energy
         */
        public int RechargeDurationSecs
        {
            get
            {
                if (Model != null && Model.GetCached(energyName) != null)
                    return Model.GetCached(energyName).RechargeRules.RechargeDuration;
                else
                    return -1;
            }
        }
        #endregion

        #region Events
        /**
         * Fires when this Energy becomes full
         */
        public event System.Action OnFull;

        /**
         * Fires when this Energy's value is updated
         * @param value New Value
         * @param prevValue Previous Value
         */
        public event Action<int, int> OnUpdated;

        /**
         * Fires when this Energy's remaining time to next energy is updated
         * @param value current remaining time
         */
        public event Action<int> OnSecondsTillIncrementUpdated;

        /**
         * Fires when an error occurs while Crediting this Energy
         * @param energy Energy Resource Component
         * @param trigger If applicable, the Energy Trigger Component used to perform the Credit operation
         * @param error The error which occurred during Credit
         */
        public event Action<Energy, EnergyTrigger, NPNFError> OnCreditError;

        /**
         * Fires when an error occurs while Debiting this Energy
         * @param energy Energy Resource Component
         * @param trigger If applicable, the Energy Trigger Component used to perform the Debit operation
         * @param error The error which occurred during Debit
         */
        public event Action<Energy, EnergyTrigger, NPNFError> OnDebitError;
        #endregion

        private bool mIsInitializing = false;
        private User mPendingUser = null;

        private void OnEnergyLoaded(NPNF.Core.EnergyModule.Energy energy, EnergyBank bank)
        {
            Model = bank;

            if (energy != null)
            {
                MinValue = energy.Bounds.Lower;
                MaxValue = energy.Bounds.Upper;
                Enabled = energy.Enabled;
                EnergyStatus status = Model.GetCached(energy.Name);
                if (status == null)
                {
                    Model.Sync(energy.Name, localNotificationMessage, (EnergyStatus newStatus, NPNFError error) => {
                        if (error != null)
                        {
                            TriggerInitializeError(error);
                        } else
                        {
                            OnEnergySynced(newStatus);
                        }
                    });
                } else
                {
                    OnEnergySynced(status);
                }
            } else
            {
                TriggerInitializeError(NPNFError.GetGameError(NPNFError.GameCode.UNKNOWN_ENERGY));
            }
        }

        private void OnEnergySynced(EnergyStatus status)
        {
            // enable energy handlers
            Model.AddValueUpdateHandler(energyName, ValueUpdateHandler);
            Model.AddMaxUnitReachedHandler(energyName, MaxUnitReachedHandler);
            Model.AddTimeUntilIncrementHandler(energyName, TimeUntilIncrementHandler);

            Value = status.CurrentEnergy;

            mIsInitializing = false;
            if (mPendingUser != null)
            {
                OnUserLoaded(mPendingUser);
            } else
            {
                TriggerInitialized();
            }
        }

        private void TimeUntilIncrementHandler(object sender, TimerArgs e)
        {
            if (OnSecondsTillIncrementUpdated != null)
            {
                OnSecondsTillIncrementUpdated(e.TimeLeft);
            }
        }

        private void ValueUpdateHandler(object sender, EnergyArgs e)
        {
            int prevValue = Value;
            Value = e.CurrentEnergy;
            if (OnUpdated != null)
            {
                OnUpdated(Value, prevValue);
            }
        }

        private void MaxUnitReachedHandler(object sender, EnergyArgs e)
        {
            if (OnFull != null)
                OnFull();
        }

        private void TriggerHandler(EnergyTrigger trigger)
        {
            if (trigger.action == EnergyTrigger.Action.Debit)
            {
                Debit(trigger.amount, trigger);
            } else if (trigger.action == EnergyTrigger.Action.Credit)
            {
                Credit(trigger.amount, trigger);
            }
        }

        /**
         * Debit an specified number of units of Energy
         * @param amount Number of units to debit
         */
        public void Debit(int amount)
        {
            Debit(amount, null);
        }

        /**
         * Credit an specified number of units of Energy
         * @param amount Number of units to credit
         */
        public void Credit(int amount)
        {
            Credit(amount, null);
        }

        private void Debit(int amount, EnergyTrigger trigger)
        {
            if (!IsInitialized)
            {
                HandleUninitializedEnergy(trigger, OnDebitError);
                return;
            }

            Model.Debit(energyName, amount, (EnergyStatus status, NPNFError error) => {
                if (error != null && OnDebitError != null)
                {
                    OnDebitError(this, trigger, error);
                }
                FinishTrigger(trigger, error);
            });
        }

        private void Credit(int amount, EnergyTrigger trigger)
        {
            if (!IsInitialized)
            {
                HandleUninitializedEnergy(trigger, OnCreditError);
                return;
            }

            Model.Credit(energyName, amount, (EnergyStatus status, NPNFError error) => {
                if (error != null && OnCreditError != null)
                {
                    OnCreditError(this, trigger, error);
                }
                FinishTrigger(trigger, error);
            });
        }

        private void HandleUninitializedEnergy(EnergyTrigger trigger, Action<Energy, EnergyTrigger, NPNFError> callback)
        {
            NPNFError error = NPNFError.GetGameError(NPNFError.GameCode.ENERGY_NOT_INITIALIZED);
            if (callback != null)
            {
                callback(this, trigger, error);
            }
            FinishTrigger(trigger, error);
        }
    }
}
