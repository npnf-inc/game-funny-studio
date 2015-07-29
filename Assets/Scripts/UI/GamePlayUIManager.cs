using UnityEngine;
using System.Collections;
using SKPlanet;
using UnityEngine.UI;
public class GamePlayUIManager : MonoBehaviourSingleton<GamePlayUIManager> {

    public Text goldText;
    public Text commitText;
    public Text productsText;

    public override void Init()
    {
        goldText.text = PlayerData.Instance.gold.ToString();
        commitText.text = PlayerData.Instance.commit.ToString();
        productsText.text = PlayerData.Instance.releaseProducts.ToString();
    }

    #region Button Event
    public void OnHireDevEvent()
    {
        PlayerData.Instance.HireNewEngineer();
    }
    #endregion

    void OnEnable()
    {
        PlayerData.Instance.OnUpdateCommitEvent += UpdateComitUI;
        PlayerData.Instance.OnUpdateGoldEvent += UpdateGoldUI;
        PlayerData.Instance.OnUpdateReleaseProductEvent += UpdateReleaseProductUI;
    }

    void OnDisable()
    {
        PlayerData.Instance.OnUpdateCommitEvent -= UpdateComitUI;
        PlayerData.Instance.OnUpdateGoldEvent -= UpdateGoldUI;
        PlayerData.Instance.OnUpdateReleaseProductEvent -= UpdateReleaseProductUI;
    }

    void UpdateComitUI()
    {
        commitText.text = PlayerData.Instance.commit.ToString();
    }

    void UpdateGoldUI()
    {
        goldText.text = PlayerData.Instance.gold.ToString();
    }

    void UpdateReleaseProductUI()
    {
        productsText.text = PlayerData.Instance.releaseProducts.ToString();
    }
}
