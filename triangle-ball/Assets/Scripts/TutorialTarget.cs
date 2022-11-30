using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTarget : MonoBehaviour
{
    public bool targetReached;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        targetReached = true;
    }
}
