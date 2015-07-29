using System;

namespace NPNF.Game
{
    /**
     * GachaTrigger - one trigger represents a price for playing a gacha. 
     * 
     * Attached this script to the gameObject which is for example a button for user to select.
     */
    [Serializable]
    public class GachaTrigger : NPNFGameTrigger<GachaTrigger, Gacha>
    {
        public string priceName;
        public int purchaseCount = 1;

        protected override void Activate()
        {
            StartTriggerAction(this);
        }
    }
}
