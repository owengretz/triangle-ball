using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UltimateReplay;

public class ColourBall : ReplayBehaviour
{
    private SpriteRenderer rend;

    private Material[] mats;

    [HideInInspector] public BallManager ballManager;

    private float[] timeOfLastTouches = new float[3] { 0f, 0f, 0f }; // blue, red, green

    [HideInInspector]
    [ReplayVar]
    public Color colour;

    private void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        mats = GameManager.instance.playerMaterials;
    }


    private void Update()
    {
        if (!ReplayManager.IsReplayingAny)
        {
            if (PlayerHit())
            {
                colour = GetLastHitMat().color;
                colour.a = 1;
            }
            else
            {
                ChangeTransparency();
            }
        }


        rend.color = colour;
    }

    private void ChangeTransparency()
    {
        colour.a -= Time.deltaTime / 5f;
    }

    private bool PlayerHit()
    {
        bool change = false;
        for (int i = 0; i < 3; i++)
        {
            if (timeOfLastTouches[i] != ballManager.timeOfLastTouches[i])
            {
                change = true;
            }
            timeOfLastTouches[i] = ballManager.timeOfLastTouches[i];
        }
        return change;
    }

    private Material GetLastHitMat()
    {
        return mats[GameManager.instance.teamIndexes[ballManager.colourOfLastTouches[0]]];
    }
}
