using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreePlayerManager : ModeManager
{
    public override void SetKickoffPositions()
    {
        List<Transform> spawns = new List<Transform>();
        for (int i = 1; i <= 3; i++)
        {
            spawns.Add(GameObject.Find("Spawn" + i.ToString()).transform);
        }

        // tell players their transforms
        for (int i = 0; i < gm.players.Count; i++)
        {
            gm.players[i].SetTransform(spawns[i]);
        }
    }
}
