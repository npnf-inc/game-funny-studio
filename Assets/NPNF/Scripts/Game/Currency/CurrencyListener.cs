using UnityEngine;
using NPNF.Core;

namespace NPNF.Game
{
    public abstract class CurrencyListener : NPNFGameListener
    {
        [SerializeField] protected Currency currency;

        protected virtual void OnEnable()
        {
            currency.OnInitialized += InitializeHandler;
            currency.OnInitializeError += InitializeErrorHandler;
            currency.OnBalanceChanged += BalanceChangedHandler;
            currency.OnCreditError += CreditErrorHandler;
            currency.OnDebitError += DebitErrorHandler;
            currency.OnConvertError += ConvertErrorHandler;
        }

        protected virtual void OnDisable()
        {
            currency.OnInitialized -= InitializeHandler;
            currency.OnInitializeError -= InitializeErrorHandler;
            currency.OnBalanceChanged -= BalanceChangedHandler;
            currency.OnCreditError -= CreditErrorHandler;
            currency.OnDebitError -= DebitErrorHandler;
            currency.OnConvertError -= ConvertErrorHandler;
        }

        protected abstract void BalanceChangedHandler(int value, int previousValue);
        protected abstract void CreditErrorHandler(Currency currency, CurrencyTrigger trigger, NPNFError error);
        protected abstract void DebitErrorHandler(Currency currency, CurrencyTrigger trigger, NPNFError error);
        protected abstract void ConvertErrorHandler(Currency currency, CurrencyTrigger trigger, NPNFError error);
    }
}
