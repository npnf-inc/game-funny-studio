using UnityEngine;
using System.Collections;
using SKPlanet;
using NPNF.Core;
using NPNF.Core.UserModule;
using NPNF.Core.EnergyModule;
using System.Collections.Generic;
using System;

public class EnergyManager : MonoBehaviourSingleton<EnergyManager> {

    
    const string COMMITS_ENERGY = "commits";
    const string BANK_ACCOUNT_ENERGY = "bank account";

    public event Action<int> OnCommitsUpdate;
    public event Action<int> OnBankAccountUpdate;

    bool isAddEvent = false;
    List<string> energyUpdateEventLst = new List<string>();

    public override void Init()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SynAllEnergy(Action callback = null)
    {
        if(User.CurrentProfile != null)
        {
            User.CurrentProfile.EnergyBank.SyncAll((Dictionary<string, EnergyStatus> enegyDict, NPNFError error) => {
                if (error == null)
                {
                    foreach (var enegy in enegyDict)
                    {
                        OnEnergyUpdate(enegy.Value.EnergyName, enegy.Value.CurrentEnergy);
                        if (isAddEvent == false)
                        {
                            isAddEvent = true;
                            User.CurrentProfile.EnergyBank.AddValueUpdateHandler(enegy.Value.EnergyName, HandleOnCurrentEnergyUpdate);
                            energyUpdateEventLst.Add(enegy.Value.EnergyName);
                        }
                    }
                }
                else
                {
                    SynAllEnergy(callback);
                }
            });
        }
    }

    void HandleOnCurrentEnergyUpdate(object sender, EnergyArgs e)
    {
        OnEnergyUpdate(e.EnergyName, e.CurrentEnergy);
    }

    void OnEnergyUpdate(string name, int total)
    {
        switch(name)
        {
            case COMMITS_ENERGY:
                if (OnCommitsUpdate != null)
                    OnCommitsUpdate(total);
                break;

            case BANK_ACCOUNT_ENERGY:
                if( OnBankAccountUpdate != null)
                    OnBankAccountUpdate(total);
                break;
        }
    }

    void OnDestroy()
    {
        if (energyUpdateEventLst != null && energyUpdateEventLst.Count > 0)
        {
            for (int i = 0; i < energyUpdateEventLst.Count; i++)
            {
                User.CurrentProfile.EnergyBank.RemoveValueUpdateHandler(energyUpdateEventLst[i], HandleOnCurrentEnergyUpdate);
            }
        }
        energyUpdateEventLst = new List<string>();
    }

    public void GetAllEnergy(Action<bool> callback = null, int retry = 3)
    {
        Energy.GetAll(false, false, (List<Energy> energyLst, NPNFError error) =>
        {
            if (error == null)
            {
                if (callback != null)
                    callback(true);
            }
            else
            {
                retry--;
                if (retry > 0)
                {
                    GetAllEnergy(callback, retry);
                }
                else
                {
                    if (callback != null)
                        callback(false);
                }
            }
        });
    }
}
