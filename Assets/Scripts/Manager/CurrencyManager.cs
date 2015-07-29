using UnityEngine;
using System.Collections;
using SKPlanet;
using NPNF.Core;
using NPNF.Core.UserModule;
using NPNF.Core.CurrencyModule;
using System.Collections.Generic;
using System;
public class CurrencyManager : MonoBehaviourSingleton<CurrencyManager> {

    const string BANK_ACCOUNT = "bank account";
    const string ENGINEERS = "engineers";
    const string COMMITS = "commits";
    const string RELEASES = "releases";

    public event Action<int> OnBankAccountUpdate;
    public event Action<int> OnEngineersUpdate;
    public event Action<int> OnCommitsUpdate;
    public event Action<int> OnReleasesUpdate;

    public override void Init()
    {
        DontDestroyOnLoad(gameObject);
    }


    public void UpdateEngineers(int amount, Action<bool> callback = null, int retry = 3)
    {
        if (amount > 0)
        {
            DepositCurrency(ENGINEERS, amount, (BankReceipt receipt) =>
            {
                if(callback != null)
                {
                    callback(receipt != null);
                }

                if(OnEngineersUpdate != null)
                {
                    OnEngineersUpdate(receipt.Balance);
                }
            }, retry);
        }
        else if (amount < 0)
        {
            ConsumeCurrency(ENGINEERS, -amount, (BankReceipt receipt) =>
            {
                if (callback != null)
                {
                    callback(receipt != null);
                }

                if (OnEngineersUpdate != null)
                {
                    OnEngineersUpdate(receipt.Balance);
                }
            }, retry);
        }

    }

    public void UpdateCommits(int amount, Action<bool> callback = null, int retry = 3)
    {
        if (amount > 0)
        {
            DepositCurrency(COMMITS, amount, (BankReceipt receipt) =>
            {
                if (callback != null)
                {
                    callback(receipt != null);
                }

                if (OnCommitsUpdate != null)
                {
                    OnCommitsUpdate(receipt.Balance);
                }
            }, retry);
        }
        else if (amount < 0)
        {
            ConsumeCurrency(COMMITS, -amount, (BankReceipt receipt) =>
            {
                if (callback != null)
                {
                    callback(receipt != null);
                }

                if (OnCommitsUpdate != null)
                {
                    OnCommitsUpdate(receipt.Balance);
                }
            }, retry);
        }
    }

    public void UpdateReleasesProduct(int amount, Action<bool> callback = null, int retry = 3)
    {
        if (amount > 0)
        {
            DepositCurrency(RELEASES, amount, (BankReceipt receipt) =>
            {
                if (callback != null)
                {
                    callback(receipt != null);
                }

                if (OnReleasesUpdate != null)
                {
                    OnReleasesUpdate(receipt.Balance);
                }
            }, retry);
        }
        else if (amount < 0)
        {
            ConsumeCurrency(RELEASES, -amount, (BankReceipt receipt) =>
            {
                if (callback != null)
                {
                    callback(receipt != null);
                }

                if (OnReleasesUpdate != null)
                {
                    OnReleasesUpdate(receipt.Balance);
                }
            }, retry);
        }
    }

    public void UpdateGold(int amount, Action<bool> callback = null, int retry = 3)
    {
        if (amount > 0)
        {
            DepositCurrency(BANK_ACCOUNT, amount, (BankReceipt receipt) =>
            {
                if (callback != null)
                {
                    callback(receipt != null);
                }

                if (OnBankAccountUpdate != null)
                {
                    OnBankAccountUpdate(receipt.Balance);
                }
            }, retry);
        }
        else if (amount < 0)
        {
            ConsumeCurrency(BANK_ACCOUNT, -amount, (BankReceipt receipt) =>
            {
                if (callback != null)
                {
                    callback(receipt != null);
                }

                if (OnBankAccountUpdate != null)
                {
                    OnBankAccountUpdate(receipt.Balance);
                }
            }, retry);
        }
    }

    // Consume Currency
    private void ConsumeCurrency(string currencyName, int amount, Action<BankReceipt> callback = null, int retry = 3)
    {
        User.CurrentProfile.CurrencyBank.Debit(currencyName, amount, (BankReceipt receipt, NPNFError error) =>
        {
            if (error == null)
            {
                if(callback != null)
                    callback(receipt);
            }
            else
            {
                retry--;
                if(retry > 0)
                {
                    ConsumeCurrency(currencyName, amount, callback, retry);
                }
                else
                {
                    if (callback != null)
                        callback(null);
                }
            }
        });
    }

    // Deposit Currency
    private void DepositCurrency(string currencyName, int amount, Action<BankReceipt> callback = null, int retry = 3)
    {
        User.CurrentProfile.CurrencyBank.Credit(currencyName, amount, (BankReceipt receipt, NPNFError error) =>
        {
            if (error == null)
            {
                if (callback != null)
                    callback(receipt);
            }
            else
            {
                retry--;
                if (retry > 0)
                {
                    DepositCurrency(currencyName, amount, callback, retry);
                }
                else
                {
                    if (callback != null)
                        callback(null);
                }
            }
        });
    }

    public void ViewAllBalance()
    {
        User.CurrentProfile.CurrencyBank.GetAllBalances(false, (Dictionary<string, BankReceipt> receipts, NPNFError error) => { 
            if(error == null)
            {
                if (receipts.ContainsKey(BANK_ACCOUNT) && OnBankAccountUpdate != null)
                {
                    OnBankAccountUpdate(receipts[BANK_ACCOUNT].Balance);
                }

                if (receipts.ContainsKey(ENGINEERS) && OnEngineersUpdate != null)
                {
                    OnEngineersUpdate(receipts[ENGINEERS].Balance);
                }

                if (receipts.ContainsKey(COMMITS) && OnCommitsUpdate != null)
                {
                    OnCommitsUpdate(receipts[COMMITS].Balance);
                }

                if (receipts.ContainsKey(RELEASES) && OnReleasesUpdate != null)
                {
                    OnReleasesUpdate(receipts[RELEASES].Balance);
                }
            }
            else
            {
                ViewAllBalance();
            }
        });
    }
}
