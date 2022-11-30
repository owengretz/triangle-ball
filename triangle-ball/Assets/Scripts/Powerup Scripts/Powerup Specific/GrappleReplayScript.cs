using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class GrappleReplayScript : ReplayBehaviour
{
    [ReplayVar]
    public float length;

    private SpriteRenderer rend;

    private void Start()
    {
        rend = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (ReplayManager.IsReplayingAny)
        {
            Transform replayBall = GameObject.FindGameObjectWithTag("Ball").transform;
            Vector2 difference = transform.position - replayBall.position;
            rend.size = new Vector2(1f, difference.magnitude);
        }
    }
}
