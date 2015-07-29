using System;
using NPNF.Core.CollectionModule;
using NPNF.Core.FusionModule;
using System.Collections.Generic;

namespace NPNF.Game
{
    [Serializable]
    public class FusionTrigger : NPNFGameTrigger<FusionTrigger, Fusion>
    {
        //A specific set of entitlements to be consumed
        public List<FuseExecutions> fusionExecutions;
        public string priceName;

        protected override void Activate()
        {
            StartTriggerAction(this);
        }
    }
}