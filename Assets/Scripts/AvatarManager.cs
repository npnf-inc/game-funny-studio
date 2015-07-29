using UnityEngine;
using System.Collections;
using SKPlanet;
using System.Collections.Generic;

public class AvatarManager : MonoBehaviourSingleton<AvatarManager> {
    const string path = "AvatarPack/";
    string[] avatarArray;

    public override void Init()
    {
        avatarArray = new string[] {
            "avatar_balletevo1_female",
            "avatar_balletevo1_male",
            "avatar_bodybuilderevo1_female",
            "avatar_bodybuilderevo1_male",
            "avatar_businessmanevo1_female",
            "avatar_businessmanevo1_male"
        };
    }

    public GameObject CreateAvatar(string avatarName = "")
    {
        avatarName = string.IsNullOrEmpty(avatarName) ? avatarArray[UnityEngine.Random.Range(0, avatarArray.Length)] : avatarName;
        GameObject obj = (GameObject)Instantiate(Resources.Load(path+avatarName) as GameObject);
        obj.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
        obj.transform.localScale = new Vector3(3, 3, 3);
        obj.name = avatarName;
        return obj;
    }
}
