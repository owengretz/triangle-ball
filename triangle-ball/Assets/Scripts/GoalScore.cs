using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScore : MonoBehaviour
{
    public GameManager.Teams team;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Ball"))
            return;

        GameManager.Teams scorer = GameManager.instance.TeamOfScorer(FindObjectOfType<BallManager>(), team);
        GameManager.TriggerGoalScored(scorer, team);
    }
}
