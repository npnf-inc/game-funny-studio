using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NPNF.Core;
using NPNF.Core.UserModule;
using NPNF.Core.CurrencyModule;

namespace NPNF.Game
{
    [Serializable]
    public class Currency : NPNFGame<CurrencyBank>
    {
        [SerializeField] private string currencyName;
        [SerializeField] private CurrencyTrigger[] triggers;

        private NPNF.Core.CurrencyModule.Currency mCurrency;

        #region Properties
        /**
         * Flag to indicate whether this Currency is enabled or disabled.
         */
        public bool Enabled { get; private set; }

        /**
         * The currenct balance of this Currency
         */
        public int Balance { get; private set; }

        /**
         * The minimum possible value for this Currency
         */
        public int MinValue { get; private set; }

        /**
         * The maximum pospsible value for this Currency
         */
        public int MaxValue { get; private set; }

        /**
         * Name of this Currency
         */
        public string Name
        {
            get
            {
                return currencyName;
            }
        }

        /**
         * The list of ExchangeRules associated with this Currency
         */
        public List<ExchangeRule> ExchangeRules
        {
            get
            {
                if (mCurrency != null)
                    return mCurrency.ExchangeRules;
                else
                    return null;
            }
        }
        #endregion

        #region Events
        /**
         * Fires when this Currency's balance changes
         * @param value New Value
         * @param prevValue Previous Value
         */
        public event Action<int, int> OnBalanceChanged;

        /**
         * Fires when an error occurs while Crediting this Currency
         * @param currency Currency Resource Component
         * @param trigger If applicable, the Currency Trigger Component used to perform the Credit operation
         * @param error The error which occurred during Credit
         */
        public event Action<Currency, CurrencyTrigger, NPNFError> OnCreditError;

        /**
         * Fires when an error occurs while Debiting this Currency
         * @param currency Currency Resource Component
         * @param trigger If applicable, the Currency Trigger Component used to perform the Debit operation
         * @param error The error which occurred during Debit
         */
        public event Action<Currency, CurrencyTrigger, NPNFError> OnDebitError;

        /**
         * Fires when an error occurs while Converting this Currency
         * @param currency Currency Resource Component
         * @param trigger If applicable, the Currency Trigger Component used to perform the Convert operation
         * @param error The error which occurred during Convert
         */
        public event Action<Currency, CurrencyTrigger, NPNFError> OnConvertError;
        #endregion

        protected override void Start()
        {
            base.Start();

            foreach (CurrencyTrigger trigger in triggers)
            {
                trigger.OnTrigger += TriggerHandler;
            }
        }

        protected override void OnUserLoaded(User user)
        {
            base.OnUserLoaded(user);

            if (Model != null)
                Model.OnUpdate -= BalanceChangedHandler;

            NPNF.Core.CurrencyModule.Currency.Get(currencyName, true, (NPNF.Core.CurrencyModule.Currency currency, NPNFError error) => {
                mCurrency = currency;
                OnCurrencyLoaded(currency, user.Profiles.Current.CurrencyBank);
            });
        }

        private void OnCurrencyLoaded(NPNF.Core.CurrencyModule.Currency currency, CurrencyBank bank)
        {
            if (currency != null && bank != null)
            {
                Model = bank;
                Model.OnUpdate += BalanceChangedHandler;
                MinValue = currency.Bounds.Lower;
                MaxValue = currency.Bounds.Upper;
                Enabled = currency.Enabled;
                bank.GetBalance(currencyName, true, (BankReceipt receipt, NPNFError getBalanceError) => {
                    if (getBalanceError == null)
                    {
						Balance = receipt.Balance;
                        TriggerInitialized();
                    } else
                    {
                        TriggerInitializeError(getBalanceError);
                    }
                });
            } else
            {
                Debug.Log("Currency not initialized properly.");
                TriggerInitializeError(NPNFError.GetGameError(NPNFError.GameCode.UNKNOWN_CURRENCY));
            }
        }

        private void NewBalanceHandler(int balance)
        {
            int prevBalance = Balance;
            Balance = balance;
            if (OnBalanceChanged != null)
            {
                OnBalanceChanged(balance, prevBalance);
            }
        }

        private void BalanceChangedHandler(BankReceipt receipt)
        {
            if (receipt.CurrencyName == mCurrency.Name)
            {
                NewBalanceHandler(receipt.Balance);
            }
        }

        private void TriggerHandler(CurrencyTrigger trigger)
        {
            if (trigger.action == NPNF.Game.CurrencyTrigger.UserAction.Debit)
            {
                Debit(trigger.amount, trigger);
            } else if (trigger.action == NPNF.Game.CurrencyTrigger.UserAction.Credit)
            {
                Credit(trigger.amount, trigger);
            }
        }

        /**
         * Consume a given amount of this currency
         * @param amount - the amount to consume
         */
        public void Debit(int amount)
        {
            Debit(amount, null);
        }

        /**
         * Deposit a given amount of this currency
         * @param amount - the amount to deposit
         */
        public void Credit(int amount)
        {
            Credit(amount, null);
        }

        /**
         * Convert this currency to another based on an ExchangeRule
         * @param multiplier - how much of this currency to convert
         * @param rule - the ExchangeRule to be used for the conversion
         */
        public void Convert(int multiplier, ExchangeRule rule)
        {
            Convert(multiplier, rule, null);
        }

        private void Debit(int amount, CurrencyTrigger trigger)
        {
            if (!IsInitialized)
            {
                HandleUninitializedCurrency(trigger, OnDebitError);
                return;
            }
            Model.Debit(currencyName, amount, (BankReceipt receipt, NPNFError error) => {
                if (error != null && OnDebitError != null)
                {
                    OnDebitError(this, trigger, error);
                }
                FinishTrigger(trigger, error);
            });
        }

        private void Credit(int amount, CurrencyTrigger trigger)
        {
            if (!IsInitialized)
            {
                HandleUninitializedCurrency(trigger, OnCreditError);
                return;
            }
            Model.Credit(currencyName, amount, (BankReceipt receipt, NPNFError error) => {
                if (error != null && OnCreditError != null)
                {
                    OnCreditError(this, trigger, error);
                }
                FinishTrigger(trigger, error);
            });
        }

        private void Convert(int multiplier, ExchangeRule rule, CurrencyTrigger trigger)
        {
            if (!IsInitialized)
            {
                HandleUninitializedCurrency(trigger, OnConvertError);
                return;
            }
            Model.Convert(currencyName, multiplier, rule.Name, (Dictionary<string, BankReceipt> receipts, NPNFError error) => {
                if (error != null && OnConvertError != null)
                {
                    OnConvertError(this, trigger, error);
                }
                FinishTrigger(trigger, error);
            });
        }

        private void HandleUninitializedCurrency(CurrencyTrigger trigger, Action<Currency, CurrencyTrigger, NPNFError> callback)
        {
            NPNFError error = NPNFError.GetGameError(NPNFError.GameCode.CURRENCY_NOT_INITIALIZED);
            if (callback != null)
            {
                callback(this, trigger, error);
            }
            FinishTrigger(trigger, error);
        }

        protected void FinishTrigger(CurrencyTrigger trigger, NPNFError error)
        {
            if (trigger != null)
            {
                trigger.FinishTriggeredAction(this, trigger, error);
            }
        }
    }
}