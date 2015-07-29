using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SKPlanet;
public class GameManager : MonoBehaviourSingleton<GameManager>
{

    GameObject engineersParentObj;
    List<GameObject> engineersLst = new List<GameObject>();
    float startPosX = -4.5f;
    float startPosY = 4f;

    public override void Init()
    {
        engineersParentObj = new GameObject("Developers");
    }

    void Start()
    {
        PlayerData.Instance.OnUpdateEngineersEvent += OnUpdateEngineersEvent;
        OnUpdateEngineersEvent();
    }

    void OnUpdateEngineersEvent()
    {
        int addedEngineers = PlayerData.Instance.totalEngineers - engineersLst.Count;
        for(int i=0; i<addedEngineers; i++)
        {
            AddNewEngineer();
        }
    }

    public void AddNewEngineer()
    {
        GameObject avatar = AvatarManager.Instance.CreateAvatar();
        avatar.transform.SetParent(engineersParentObj.transform);
        avatar.transform.localPosition = GetDeveloperPosition();
        engineersLst.Add(avatar);
    }

    Vector3 GetDeveloperPosition()
    {
        Vector3 pos = new Vector3(startPosX, startPosY, 0);
        startPosX += 1f;
        if (startPosX > 4.5f)
        {
            startPosX = -4.5f;
            startPosY -= 1f;
        }

        return pos;
    }
}
