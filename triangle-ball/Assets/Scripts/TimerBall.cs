using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UltimateReplay;

public class TimerBall : ReplayBehaviour
{
    [HideInInspector]
    [ReplayVar]
    public string clockTextString;

    private TMP_Text clockText;


    private void Start()
    {
        clockText = GetComponentInChildren<TMP_Text>();
    }

    private void Update()
    {
        if (!ReplayManager.IsReplayingAny)
        {
            clockTextString = TimeManager.instance.clockText.text;
        }

        clockText.text = clockTextString;
    }
}
