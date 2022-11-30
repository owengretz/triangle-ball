using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowmoButton : MonoBehaviour
{
    bool slow = false;

    public void Slowmo()
    {
        if (slow) Time.timeScale = 1f;
        else Time.timeScale = 0.2f;
        slow = !slow;
    }
}
