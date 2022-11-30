using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class SpikesPowerup : Powerup
{
    private GameObject ball;
    private GameObject spikes;
    private SpikesBallScript spikeBall;
    private PlayerPowerupScript pScript;
    private float mass;
    private float linearDrag;
    private float angularDrag;
    private float gravityScale;
    private CollisionDetectionMode2D collisionDetection;
    private RigidbodyInterpolation2D interpolation;

    private bool releaseBall;

    [HideInInspector] public Vector2 ballVelocity; // public to let BotUtils acces
    private Vector3 ballPos;
    [HideInInspector] public float timeRemaining; // public to let SPikesBallScript access

    private BallManager ballManager;

    public override void UsePowerup()
    {
        base.UsePowerup();
        //duration = 7f;
        timeRemaining = GameInfo.spikesDuration != -1 ? GameInfo.spikesDuration : 9999f;

        spikes = Instantiate(PowerupManager.instance.spikes, transform);
        ReplayManager.AddReplayObjectToRecordScenes(spikes.GetComponent<ReplayObject>());

        ballPos = Vector2.zero;

        pScript = GetComponent<PlayerPowerupScript>();

        if (FindObjectOfType<BallManager>() == null) return;

        ball = FindObjectOfType<BallManager>().gameObject;
        StartCoroutine(WaitForTouch());
    }

    // waits until the player touches the ball then sticks it
    private IEnumerator WaitForTouch()
    {
        spikeBall = ball.gameObject.AddComponent<SpikesBallScript>();
        spikeBall.playerNumber = GetComponent<PlayerManager>().playerNumber;

        while (timeRemaining > 0f && !spikeBall.firstTouch && ball != null)
        {
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        if (ball != null && timeRemaining > 0f)
        {
            ball.transform.SetParent(transform, true);
            ballManager = GetComponentInChildren<BallManager>();
            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            mass = rb.mass;
            linearDrag = rb.drag;
            angularDrag = rb.angularDrag;
            gravityScale = rb.gravityScale;
            collisionDetection = rb.collisionDetectionMode;
            interpolation = rb.interpolation;
            Destroy(rb);
        }

        StartCoroutine(Use());
    }

    // powerup timer
    private IEnumerator Use()
    {
        while (timeRemaining > 0f && !spikeBall.hitOff && spikeBall.firstTouch && ball != null && !releaseBall)
        {
            if (pScript.isPowerupButtonPressed) releaseBall = true;

            timeRemaining -= Time.deltaTime;
            yield return null;
        }
        endPowerup = true;
    }
    // calculate velocity in fixed update to apply to the rigidbody on release
    private void FixedUpdate()
    {
        if (ball != null)
        {
            ballVelocity = (ball.transform.position - ballPos) / Time.fixedDeltaTime;
            ballPos = ball.transform.position;
        }

        if (ballManager != null)
            ballManager.velocity = ballVelocity;
    }
    // if player gets demoed while using spikes
    public override void Die()
    {
        base.Die();
        if (ball != null) ball.transform.position = ballPos;
    }

    public override void EndPowerup()
    {
        base.EndPowerup();

        if (ball != null && ball.GetComponent<Rigidbody2D>() == null)
        {
            ball.transform.SetParent(null, true);
            Rigidbody2D rb = ball.AddComponent<Rigidbody2D>();
            ballManager.rb = rb;
            rb.mass = mass;
            rb.drag = linearDrag;
            rb.angularDrag = angularDrag;
            rb.gravityScale = gravityScale;
            rb.collisionDetectionMode = collisionDetection;
            rb.interpolation = interpolation;
            if (releaseBall) rb.velocity = ballVelocity + ballVelocity.normalized * GameInfo.spikesReleaseSpeed;
            else rb.velocity = ballVelocity * 1.2f;
            Destroy(spikeBall);
        }
        if (spikes != null)
        {
            ReplayManager.RemoveReplayObjectFromRecordScenes(spikes.GetComponent<ReplayObject>());
            Destroy(spikes);
        }
    }
}
