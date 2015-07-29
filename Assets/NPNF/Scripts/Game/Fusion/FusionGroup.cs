using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using NPNF.Core;
using NPNF.Core.CollectionModule;

namespace NPNF.Game
{
    public class FusionGroup : NPNFGameGroup<Fusion, NPNF.Core.FusionModule.Fusion>
    {
        protected override void OnPlatformInit()
        {
            base.OnPlatformInit();
            NPNF.Core.FusionModule.Fusion.GetAll(true, (List<NPNF.Core.FusionModule.Fusion> formulas, NPNFError error) => {
                if (error == null)
                {
                    TriggerInitialized(formulas);
                } else
                {
                    TriggerInitializeError(error);
                }
            });
        }

        protected override void ConfigureComponent(GameObject gameObject, Fusion component, NPNF.Core.FusionModule.Fusion model)
        {
            component.formulaName = model.Name;
        }
    }
}
