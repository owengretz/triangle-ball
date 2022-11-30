using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class HijackBallPowerup : Powerup
{
    private GameObject hijackEffect;
    private PlayerManager manager;
    private GameObject ball;
    //private readonly float moveForce = 3.2f;

    private string horInputName;
    private string vertInputName;
    // used by BotScript
    [HideInInspector] public float horInputValue;
    [HideInInspector] public float vertInputValue;

    private bool skippedPowerup;

    public override void UsePowerup()
    {
        //duration = 4f;

        manager = GetComponent<PlayerManager>();

        if (FindObjectOfType<BallManager>() == null)
        {
            skippedPowerup = true;
            endPowerup = true;
            return;
        }

        manager.isDead = true;

        ball = FindObjectOfType<BallManager>().gameObject;

        horInputName = "Horizontal " + (manager.playerNumber + 1);
        vertInputName = "Vertical " + (manager.playerNumber + 1);

        GetComponent<PlayerMovement>().canMove = false;

        GameObject effect = Instantiate(manager.demoExplosionPrefab, transform.position, Quaternion.identity);
        ReplayManager.AddReplayObjectToRecordScenes(effect.GetComponent<ReplayObject>());
        transform.position = Vector2.one * 100f;

        hijackEffect = Instantiate(PowerupManager.instance.hijackBall, ball.transform);
        hijackEffect.GetComponentInChildren<Renderer>().material = manager.playerMat;
        ReplayManager.AddReplayObjectToRecordScenes(hijackEffect.GetComponent<ReplayObject>());


        StartCoroutine(Control());
    }

    private IEnumerator Control()
    {
        float timer = GameInfo.hijackDuration;
        while (timer > 0f && ball != null)
        {
            if (!manager.isBot)
            {
                horInputValue = Sinput.GetAxisRaw(horInputName);
                vertInputValue = Sinput.GetAxisRaw(vertInputName);
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        endPowerup = true;
    }

    private void FixedUpdate()
    {
        Vector2 moveForce = new Vector2(horInputValue, vertInputValue) * GameInfo.hijackMoveForce * ((1 + GameInfo.ballMass) / 2f) * Time.fixedDeltaTime;
        if (ball.GetComponent<Rigidbody2D>() != null)
        {
            ball.GetComponent<Rigidbody2D>().AddForce(moveForce, ForceMode2D.Impulse);
        }
        else
        {
            ball.transform.parent.GetComponent<Rigidbody2D>().AddForce(moveForce * 1.5f, ForceMode2D.Impulse);
        }
    }

    public override void EndPowerup()
    {
        GetComponent<PlayerMovement>().canMove = true;
        manager.isDead = false;

        if (skippedPowerup) return;

        int respawnIndex = Random.Range(0, 2);
        string respawnPosName = manager.team + "Respawn" + respawnIndex;
        manager.SetTransform(GameObject.Find(respawnPosName).transform);

        GameObject effect = Instantiate(manager.respawnEffectPrefab, transform);
        effect.GetComponentInChildren<Renderer>().material = manager.playerMat;
        ReplayManager.AddReplayObjectToRecordScenes(effect.GetComponent<ReplayObject>());

        if (ball == null) return;

        hijackEffect.GetComponentInChildren<ParticleSystem>().Stop();
        hijackEffect.AddComponent<DestroyObject>();
    }
}
