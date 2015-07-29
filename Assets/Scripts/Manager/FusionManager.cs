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

    public int commitToReleaseProduct = 0;
    public int goldToEngineer = 0;
    public override void Init()
    {
        
    }

    public void GetAllFusion(Action<bool> callback = null, int retry = 3)
    {
        Fusion.GetAll(false, (List<Fusion> fusions, NPNFError error) =>
        {
            if (error == null)
            {
                for (int i = 0; i < fusions.Count; i++)
                {
                    if (fusions[i].Name.Equals(GENERATE_RELEASE_PRODUCT))
                        commitToReleaseProduct = fusions[i].Prices[0].Currencies[0].Count;
                    else if (fusions[i].Name.Equals(HIRE_NEW_ENGINEER_FUSION))
                        goldToEngineer = fusions[i].Prices[0].Currencies[0].Count;
                }
                if (callback != null)
                    callback(true);
            }
            else
            {
                retry--;
                if (retry > 0)
                {
                    GetAllFusion(callback, retry);
                }
                else
                {
                    if (callback != null)
                        callback(false);
                }
            }
        });
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

    public void GenerateNewReleaseProduct(int amount, Action<bool> callback)
    {
        Fusion.Get(GENERATE_RELEASE_PRODUCT, true, (Fusion fusion, NPNFError getError) =>
        {
            if (getError == null)
            {
                User.CurrentProfile.Fusion.Fuse(GENERATE_RELEASE_PRODUCT, fusion.Prices[0].Name, null, amount, (FormulaResult result, NPNFError error) =>
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
