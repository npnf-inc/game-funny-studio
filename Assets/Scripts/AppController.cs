using UnityEngine;
using System.Collections;
using SKPlanet;
using System.Collections.Generic;
using NPNF.Core;
using NPNF.Core.UserModule;
using NPNF.Core.CurrencyModule;
using NPNF.Core.FormulaModule;
public class AppController : MonoBehaviourSingleton<AppController> {
    public bool isInited = false;
    public override void Init()
    {
       
    }

    void Start()
    {
        isInited = false;
        NPNF.Platform.Init((NPNFError error) =>
        {
            OnPlatfomInit(error == null);
            if (error != null)
            {
                Debug.Log("Platform Init unsuccessful: " + error.Messages[0]);
            }
            else
            {
                Debug.Log("Platform Init successfully");
            }
        });
    }

    void OnPlatfomInit(bool isSucceeded)
    {
        if(isSucceeded)
        {
            if (User.IsCurrentExist())
            {
                OnAfterLogin();
            }
            else
            {
                CreateAnonymousUser();
            }
        }
    }

    void CreateAnonymousUser()
    {
        Dictionary<string, object> custom = new Dictionary<string,object>();
        User.CreateAnonymous(custom, (User user, NPNFError error) => { 
            if(error == null)
            {
                OnAfterLogin();
            }
            else
            {
                CreateAnonymousUser();
            }
        });
    }

    void OnAfterLogin()
    {
        if(User.CurrentProfile != null)
        {
            int getDataCount = 0;
            FusionManager.Instance.GetAllFusion((bool isSucceeded) => {
                getDataCount++;
                if (getDataCount == 3)
                    isInited = true;
            });

            EnergyManager.Instance.GetAllEnergy((bool isSucceeded) =>
            {
                getDataCount++;
                if (getDataCount == 3)
                    isInited = true;
            });

            CurrencyManager.Instance.GetAllCurency((bool isSucceeded) =>
            {
                getDataCount++;
                if (getDataCount == 3)
                    isInited = true;
            });
        }
    }
}
