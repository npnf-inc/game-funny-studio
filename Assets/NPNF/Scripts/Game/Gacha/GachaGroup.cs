using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using NPNF.Core.GachaModule;
using NPNF.Core;
using System;

namespace NPNF.Game
{
    /**
     * GachaGroup - Attached this script to the gameObject which will exist in the page showing gacha.
     * 
     * This script will automatically retreives all gacha information from npnf Platform and
     * attached the gacha script to the new gameObject created according to the gacha information and the UI template provided.
     */
    public class GachaGroup : NPNFGameGroup<Gacha, NPNF.Core.GachaModule.Gacha>
    {
        public bool currentGachaOnly;
        public string filterByTag;
        
        protected override void OnPlatformInit()
        {
            base.OnPlatformInit();
            Refresh();
        }
        
        protected override void ConfigureComponent(GameObject gameObject, Gacha component, NPNF.Core.GachaModule.Gacha model)
        {
            component.gachaName = model.Name;
        }

        protected override void Refresh()
        {
            NPNF.Core.GachaModule.Gacha.GetAll(true, (List<NPNF.Core.GachaModule.Gacha> gachas, NPNFError error)=>{
                if (error == null)
                {
                    if (currentGachaOnly)
                    {
                        gachas.RemoveAll((NPNF.Core.GachaModule.Gacha gacha) => {
                            return !gacha.IsCurrent;
                        });
                    }
                    if (!String.IsNullOrEmpty(filterByTag))
                    {
                        gachas.RemoveAll((NPNF.Core.GachaModule.Gacha gacha)=>{
                            return !gacha.Tags.Contains(filterByTag);
                        });
                    }
                    TriggerInitialized(gachas);
                } else
                {
                    TriggerInitializeError(error);
                }
            });
        }
    }
}