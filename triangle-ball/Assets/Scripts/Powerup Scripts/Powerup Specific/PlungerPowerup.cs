using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class PlungerPowerup : Powerup
{
    private Transform plunger;
    //private readonly float powerupDuration = 1.5f;
    //private readonly float pullForce = 10f;
    private float range;
    //private readonly static float latchSpeed = 30f;
    private bool pulling;

    private BallManager ball;
    private Vector2 pullDir;

    public override void UsePowerup()
    {
        base.UsePowerup();

        if (GameManager.instance.state == GameManager.State.GoalScored)
        {
            EndPowerup();
            return;
        }

        range = GameInfo.plungerRange != -1 ? GameInfo.plungerRange : 9999f;
        //duration = GameInfo.plungerDuration;

        plunger = Instantiate(PowerupManager.instance.plunger, transform).transform;
        ReplayManager.AddReplayObjectToRecordScenes(plunger.GetComponent<ReplayObject>());


        StartCoroutine(Deploy());
    }
    // effect of plunger extending/shooting towards ball (this way you can visually see if the ball was out of range)
    private IEnumerator Deploy()
    {
        ball = FindObjectOfType<BallManager>();
        SpriteRenderer rend = plunger.GetComponent<SpriteRenderer>();
        float length = 0f;
        float targetLength = range;
        while (length < targetLength && ball != null)
        {
            length += GameInfo.plungerLatchSpeed * Time.deltaTime;
            rend.size = new Vector2(rend.size.x, length);

            Vector2 difference = CalculatePlunger();

            targetLength = difference.magnitude < range ? difference.magnitude : range;

            yield return null;
        }
        StartCoroutine(Use());
    }
    // keeps track of duration of plunger as well as constantly updates the visuals
    private IEnumerator Use()
    {
        pulling = true;

        float timer = GameInfo.plungerDuration;
        while (timer > 0f && ball != null)
        {
            if ((transform.position - ball.transform.position).magnitude > range) endPowerup = true;
            else
            {
                Vector2 difference = CalculatePlunger();
                plunger.GetComponent<SpriteRenderer>().size = new Vector2(1f, difference.magnitude);
            }

            timer -= Time.deltaTime;
            yield return null;
        }
        endPowerup = true;
    }
    // we do all of this while deploying and using so made it a separate func
    private Vector2 CalculatePlunger()
    {
        if (ball == null) return Vector2.zero;
        Vector2 difference = transform.position - ball.transform.position;
        pullDir = difference.normalized;
        difference = pullDir * (difference.magnitude - ball.GetComponent<CircleCollider2D>().radius / 2f);
        plunger.rotation = Quaternion.Euler(new Vector3(0f, 0f, (Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg) + 90f));
        return difference;
    }
    // cancels plunger if ball is scored
    private void Update()
    {
        ball = FindObjectOfType<BallManager>();

        if (ball == null)
        {
            endPowerup = true;
            return;
        }
    }
    // pulling the ball (physics goes in fixed update)
    private void FixedUpdate()
    {
        if (ball != null && (transform.position - ball.transform.position).magnitude <= range && pulling)
        {
            if (ball.GetComponent<Rigidbody2D>() != null)
            {
                ball.GetComponent<Rigidbody2D>().AddForce(pullDir * GameInfo.plungerForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
            }
            else
            {
                ball.transform.parent.GetComponent<Rigidbody2D>().AddForce(pullDir * GameInfo.plungerForce * 1.5f * Time.fixedDeltaTime, ForceMode2D.Impulse);
            }
        }
    }

    public override void EndPowerup()
    {
        base.EndPowerup();
        StopAllCoroutines();
        if (plunger != null)
        {
            ReplayManager.RemoveReplayObjectFromRecordScenes(plunger.GetComponent<ReplayObject>());
            Destroy(plunger.gameObject);
        }
    }
}
