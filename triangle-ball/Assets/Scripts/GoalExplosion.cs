using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalExplosion : MonoBehaviour
{
    public float force = 2500f;
    public float radius = 14f;

    private void Start()
    {
        if (GameInfo.disableGoalReset && !TimeManager.instance.overtime && TimeManager.instance.clockValue != 0f)
            return;

        Rigidbody2D[] rbs = FindObjectsOfType<Rigidbody2D>();
        foreach (Rigidbody2D rb in rbs)
        {
            rb.AddExplosionForce(force, transform.position, radius);
        }
    }
}
