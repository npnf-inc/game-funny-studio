using UnityEngine;
using System.Collections;
using SKPlanet;

using NPNF.Core;
using NPNF.Core.UserModule;
using NPNF.Core.FusionModule;
using NPNF.Core.FormulaModule;

using System;
using System.Collections.Generic;

public class FusionManager : MonoBehaviourSingleton<FusionManager> {

    const string ADD_COMMIT_FUSION = "add commit";
    const string HIRE_NEW_ENGINEER_FUSION = "recruit engineer";
    const string GENERATE_RELEASE_PRODUCT = "generate release";

    public override void Init()
    {
        
    }

    public void AddFreeCommit(Action<bool> callback)
    {
        Fusion.Get(ADD_COMMIT_FUSION, true, (Fusion fusion, NPNFError getError) => {
            if (getError == null)
            {
                User.CurrentProfile.Fusion.Fuse(ADD_COMMIT_FUSION, fusion.Prices[0].Name, null, 1, (FormulaResult result, NPNFError error) =>
                {
                    if (error == null)
                    {
                        if (callback != null)
                            callback(true);
                    }
                    else
                    {
                        if (callback != null)
                            callback(false);
                        Debug.LogError("add commit error: " + error.ToString());
                    }
                });
            }
            else
            {
                if (callback != null)
                    callback(false);
            }
        });
    }

    public void HireNewEngineer(Action<bool> callback)
    {
        Fusion.Get(HIRE_NEW_ENGINEER_FUSION, true, (Fusion fusion, NPNFError getError) =>
        {
            if (getError == null)
            {
                User.CurrentProfile.Fusion.Fuse(HIRE_NEW_ENGINEER_FUSION, fusion.Prices[0].Name, null, 1, (FormulaResult result, NPNFError error) =>
                {
                    if (error == null)
                    {
                        if (callback != null)
                            callback(true);
                    }
                    else
                    {
                        if (callback != null)
                            callback(false);
                        Debug.LogError("HireNewEngineer error: " + error.ToString());
                    }
                });
            }
            else
            {
                if (callback != null)
                    callback(false);
            }
        });
    }

    public void GenerateNewReleaseProduct(Action<bool> callback)
    {
        Fusion.Get(HIRE_NEW_ENGINEER_FUSION, true, (Fusion fusion, NPNFError getError) =>
        {
            if (getError == null)
            {
                User.CurrentProfile.Fusion.Fuse(GENERATE_RELEASE_PRODUCT, fusion.Prices[0].Name, null, 1, (FormulaResult result, NPNFError error) =>
                {
                    if (error == null)
                    {
                        if (callback != null)
                            callback(true);
                    }
                    else
                    {
                        if (callback != null)
                            callback(false);
                        Debug.LogError("generate release error: " + error.ToString());
                    }
                });
            }
            else
            {
                if (callback != null)
                    callback(false);
            }
        });
    }
}
