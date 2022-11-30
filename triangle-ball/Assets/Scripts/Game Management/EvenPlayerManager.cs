using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvenPlayerManager : ModeManager
{

    public override void SetKickoffPositions()
    {
        List<Transform> spawns = new List<Transform>();
        for (int i = 1; i <= 6; i++)
        {
            spawns.Add(GameObject.Find("Spawn" + i.ToString()).transform);
        }

        int index1 = Random.Range(0, 3);
        if (gm.straightKickoffOnly) index1 = 2;
        Transform[] spawn1 = new Transform[2] { spawns[index1], spawns[index1 + 3] };


        spawns.RemoveAt(index1 + 3);
        spawns.RemoveAt(index1);

        int index2 = Random.Range(0, 2);
        Transform[] spawn2 = new Transform[2] { spawns[index2], spawns[index2 + 2] };

        // tell players their transforms
        for (int i = 0; i < gm.players.Count; i++)
        {
            int spawn = gm.players[i].team == GameManager.Teams.Blue ? 0 : 1;
            if (gm.players[i].localPlayerNumber == 0)
            {
                gm.players[i].SetTransform(spawn1[spawn]);
            }
            else if (gm.players[i].localPlayerNumber == 1)
            {
                gm.players[i].SetTransform(spawn2[spawn]);
            }
            else
            {
                Debug.LogError("Local player number is not 0 or 1");
            }
        }
    }


    //public override void GoalScored(GameManager.Teams scoredAgainst, GameObject ball)
    //{
    //    base.GoalScored(scoredAgainst, ball);
    //}
}
