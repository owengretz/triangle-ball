using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class GrapplePowerup : Powerup
{
    private Transform grapplingHook;
    //private readonly static float latchSpeed = 40f;
    //private readonly float grappleVelocity = 10f;

    private bool hooked;
    private bool touched;

    private BallManager ball;
    private SpriteRenderer rend;


    public override void UsePowerup()
    {
        base.UsePowerup();

        if (GameManager.instance.state == GameManager.State.GoalScored)
        {
            EndPowerup();
            return;
        }

        grapplingHook = Instantiate(PowerupManager.instance.grapple, transform).transform;
        ReplayManager.AddReplayObjectToRecordScenes(grapplingHook.GetComponent<ReplayObject>());
        rend = grapplingHook.GetComponent<SpriteRenderer>();

        StartCoroutine(Deploy());
    }


    private IEnumerator Deploy()
    {
        ball = FindObjectOfType<BallManager>();
        float length = 0f;
        while (!hooked && ball != null)
        {
            length += GameInfo.grappleLatchSpeed * Time.deltaTime;
            rend.size = new Vector2(rend.size.x, length);
            CalculateGrapple();

            hooked = length >= (transform.position - ball.transform.position).magnitude;

            yield return null;
        }
        StartCoroutine(Use());
    }

    private IEnumerator Use()
    {
        while (touched == false && ball != null)
        {
            Vector2 difference = CalculateGrapple();
            Vector2 dir = -CalculateGrapple().normalized;
            GetComponent<Rigidbody2D>().velocity = dir * GameInfo.grappleGrappleSpeed;
            rend.size = new Vector2(rend.size.x, difference.magnitude);
            yield return null;
        }
        endPowerup = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball")) touched = true;
    }

    // we do all of this while deploying and using so made it a separate func
    private Vector2 CalculateGrapple()
    {
        Vector2 difference = transform.position - ball.transform.position;
        grapplingHook.rotation = Quaternion.Euler(new Vector3(0f, 0f, (Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg) + 90f));
        return difference;
    }

    // cancels if ball is scored
    private void Update()
    {
        ball = FindObjectOfType<BallManager>();

        if (ball == null)
        {
            endPowerup = true;
            return;
        }
    }

    public override void EndPowerup()
    {
        base.EndPowerup();
        StopAllCoroutines();
        if (grapplingHook != null)
        {
            ReplayManager.RemoveReplayObjectFromRecordScenes(grapplingHook.GetComponent<ReplayObject>());
            Destroy(grapplingHook.gameObject);
        }
    }
}
