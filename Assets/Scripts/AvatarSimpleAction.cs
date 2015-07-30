using UnityEngine;
using System.Collections;

public class AvatarSimpleAction : MonoBehaviour {

    string[] avatarAnimationArray;
    Animator animator;
    float randomAnimationRunning = 10f;
    const int randomAnimationTime = 10;
    public void InitAnimations()
    {
        avatarAnimationArray = new string[]
        {
            "avatar_talking",
            "avatar_idle01",
            "avatar_idle02",
            "avatar_idle03",
            "avatar_idle04",
            "avatar_idle05",
        };
        animator = this.GetComponent<Animator>();
        animator.runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load("Animation/avatar_plaza_controllerv2");
        SetRandomAvatarAnimation();
    }

    // Use this for initialization
    void Start()
    {
        InitAnimations();
    }

    // Update is called once per frame
    void Update()
    {
        randomAnimationRunning -= Time.deltaTime;
        if(randomAnimationRunning <= 0)
        {
            SetRandomAvatarAnimation();
        }
    }

    void SetRandomAvatarAnimation()
    {
        randomAnimationRunning = randomAnimationTime;
        // Reset All state
        for (int i = 0; i < avatarAnimationArray.Length; i++)
        {
            animator.SetBool(avatarAnimationArray[i], false);
        }

        animator.SetBool(avatarAnimationArray[UnityEngine.Random.Range(0, avatarAnimationArray.Length)], true);
    }
}
