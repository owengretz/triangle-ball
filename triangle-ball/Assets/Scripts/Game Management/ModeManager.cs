using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;


public class ModeManager : MonoBehaviour
{
    [HideInInspector] public GameManager gm;

    public void Setup()
    {
        gm = GameManager.instance;
    }

    public void SetColours()
    {
        //Color[] playerColours = new Color[2] { new Color(0f, 168f / 255f, 255f / 255f), new Color(254f / 255f, 93f / 255f, 93f / 255f) };

        for (int i = 0; i < gm.players.Count; i++)
        {
            gm.players[i].GetComponent<SpriteRenderer>().material = gm.playerMaterials[i];
        }

    }

    public virtual void SetKickoffPositions() {  }

    public virtual void GoalScored(GameManager.Teams scorer, GameObject ball)
    {
        ball.GetComponent<BallManager>().Explode(scorer);

        // update score
    }
}
