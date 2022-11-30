using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scenario : MonoBehaviour
{
    [Range(0, 3)]
    public int startingBoost;

    public Transform ballPos;
    public Transform botPos;

    public Vector2 ballVel;
    public Vector2 botVel;
}
