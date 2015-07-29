using UnityEngine;
using System.Collections;
using SKPlanet;
using System;

public class PlayerData : MonoBehaviourSingleton<PlayerData> {

    public override void Init()
    {
        RegisterEvent(true);
        DontDestroyOnLoad(gameObject);
    }

    public int gold;
    public int commit;
    public int totalEngineers;
    public int releaseProducts;

    public event Action OnUpdateReleaseProductEvent;
    public event Action OnUpdateCommitEvent;
    public event Action OnUpdateGoldEvent;
    public event Action OnUpdateEngineersEvent;

    void Start()
    {
        GetAllData();
    }

    public void GetAllData()
    {
        CurrencyManager.Instance.ViewAllBalance();
        EnergyManager.Instance.SynAllEnergy();
    }

    public void HireNewEngineer()
    {
        FusionManager.Instance.HireNewEngineer((bool isSucceeded) => { });
    }

    void RegisterEvent(bool isRegister)
    {
        if (isRegister)
        {
            EnergyManager.Instance.OnBankAccountUpdate += OnGoldUpdateEvent;
            EnergyManager.Instance.OnCommitsUpdate += OnCommitsUpdateEvent;
            CurrencyManager.Instance.OnEngineersUpdate += OnEngineersUpdateEvent;
            CurrencyManager.Instance.OnReleasesUpdate += OnReleasesUpdateEvent;
        }
        else
        {
            EnergyManager.Instance.OnBankAccountUpdate -= OnGoldUpdateEvent;
            EnergyManager.Instance.OnCommitsUpdate -= OnCommitsUpdateEvent;
            CurrencyManager.Instance.OnEngineersUpdate -= OnEngineersUpdateEvent;
            CurrencyManager.Instance.OnReleasesUpdate -= OnReleasesUpdateEvent;
        }
    }


    void OnGoldUpdateEvent(int total)
    {
        gold = total;
        if (OnUpdateGoldEvent != null)
            OnUpdateGoldEvent();
    }

    void OnCommitsUpdateEvent(int total)
    {
        commit = total;
        if (OnUpdateCommitEvent != null)
            OnUpdateCommitEvent();
    }

    void OnEngineersUpdateEvent(int total)
    {
        totalEngineers = total;
        if (OnUpdateEngineersEvent != null)
            OnUpdateEngineersEvent();
    }

    void OnReleasesUpdateEvent(int total)
    {
        releaseProducts = total;
        if (OnUpdateReleaseProductEvent != null)
            OnUpdateReleaseProductEvent();
    }

    void OnDestroy()
    {
        RegisterEvent(false);
    }
}
