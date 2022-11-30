using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class BallSprite : MonoBehaviour
{
    private SpriteRenderer rend;

    public GameObject eyeballSkin;
    public GameObject colourBallSkin;
    public GameObject timerBallSkin;

    private void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        if (GameInfo.specialBallIndex == -1)
        {
            rend.sprite = GameInfo.ballSkin;
        }
        else
        {
            //anim.SetInteger("Skin", -1);
            switch (GameInfo.specialBallIndex)
            {
                case 0:
                    GameObject eyeball = Instantiate(eyeballSkin, transform);
                    eyeball.GetComponent<EyeballBall>().ballTrans = transform;
                    ReplayManager.AddReplayObjectToRecordScenes(eyeball.GetComponent<ReplayObject>());
                    break;
                case 1:
                    GameObject colourBall = Instantiate(colourBallSkin, transform);
                    colourBall.GetComponent<ColourBall>().ballManager = GetComponent<BallManager>();
                    ReplayManager.AddReplayObjectToRecordScenes(colourBall.GetComponent<ReplayObject>());
                    break;
                case 2:
                    ReplayManager.AddReplayObjectToRecordScenes(Instantiate(timerBallSkin, transform).GetComponent<ReplayObject>());
                    break;
                default:
                    Debug.LogError("There is no special ball specified at index " + GameInfo.specialBallIndex);
                    break;
            }
        }
    }
}
