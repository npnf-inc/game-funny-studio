using NPNF.Core.UserModule;
using System;
using UnityEngine;

namespace NPNF.Game
{
    [Serializable]
    public class CurrencyTrigger : NPNFGameTrigger<CurrencyTrigger, Currency>
    {
        [Serializable]
        public enum UserAction {Debit, Credit};
        public UserAction action;
        public int amount;

        protected override void Activate()
        {
            StartTriggerAction(this);
        }
    }
}
