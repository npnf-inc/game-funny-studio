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

    List<string> energyUpdateEventLst;

    public override void Init()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SynAllEnergy()
    {
        if(User.CurrentProfile != null)
        {
            if (energyUpdateEventLst != null && energyUpdateEventLst.Count > 0)
            {
                for(int i=0; i<energyUpdateEventLst.Count; i++)
                {
                    User.CurrentProfile.EnergyBank.RemoveValueUpdateHandler(energyUpdateEventLst[i], HandleOnCurrentEnergyUpdate);
                }
            }
            energyUpdateEventLst = new List<string>();

            User.CurrentProfile.EnergyBank.SyncAll((Dictionary<string, EnergyStatus> enegyDict, NPNFError error) => {
                if (error == null)
                {
                    foreach (var enegy in enegyDict)
                    {
                        OnEnergyUpdate(enegy.Value.EnergyName, enegy.Value.CurrentEnergy);
                        User.CurrentProfile.EnergyBank.AddValueUpdateHandler(enegy.Value.EnergyName, HandleOnCurrentEnergyUpdate);
                        energyUpdateEventLst.Add(enegy.Value.EnergyName);
                    }
                }
                else
                {
                    SynAllEnergy();
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
}
