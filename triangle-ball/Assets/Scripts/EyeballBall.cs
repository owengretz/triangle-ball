using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeballBall : MonoBehaviour
{
    [HideInInspector] public Transform ballTrans;

    private PlayerManager[] players;

    private void Start()
    {
        players = GameManager.instance.players.ToArray();
    }

    private void Update()
    {
        transform.position = ballTrans.position + GetOffset();
    }


    private Vector3 GetOffset()
    {
        Vector3 pos = transform.position - ballTrans.position;
        Vector3 target = (GetTarget() - transform.position).normalized / 15f;


        return Vector3.Lerp(pos, target, 0.15f);
    }

    private Vector3 GetTarget()
    {
        Vector3 closestPos = Vector3.zero;
        float closestDist = 10000f;
        float secondClosestDist = 10000f;
        for (int i = 0; i < players.Length; i++)
        {
            Vector3 pos = players[i].transform.position;
            float distToPlayer = (pos - transform.position).magnitude;

            if (distToPlayer < closestDist)
            {
                closestPos = pos;
                closestDist = distToPlayer;
            }
            else if (distToPlayer < secondClosestDist)
            {
                secondClosestDist = distToPlayer;
            }
        }
        if (Mathf.Abs(closestDist - secondClosestDist) < 1f)
        {
            return ballTrans.position;
        }
        else
        {
            return closestPos;
        }
    }
}
