using UnityEngine;
using System.Collections;

public class AppSceneUIManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnGamePlayEvent()
    {
        Application.LoadLevel("GamePlay");
    }
}
