using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transitions : MonoBehaviour
{
    public static Transitions instance;
    private Animator anim;

    public delegate void Function();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Fade(Function FuncToCall = null)
    {
        anim.SetTrigger("Fade");

        if (FuncToCall != null) StartCoroutine(CallFunction(FuncToCall));
    }

    public IEnumerator CallFunction(Function FuncToCall)
    {
        yield return new WaitForSeconds(0.5f);

        FuncToCall();
    }
}
