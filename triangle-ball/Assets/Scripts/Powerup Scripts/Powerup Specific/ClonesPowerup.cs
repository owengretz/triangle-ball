using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class ClonesPowerup : Powerup
{
    private PlayerMovement mainMovement;
    private int boostsLastFrame;


    [HideInInspector] public List<PlayerMovement> clones = new List<PlayerMovement>(); // accessed by ClonesDeathScript

    public override void UsePowerup()
    {
        base.UsePowerup();

        mainMovement = GetComponent<PlayerMovement>();
        boostsLastFrame = mainMovement.BoostsAvailable();

        clones.Add(GetComponent<PlayerMovement>());
        for (int i = -1; i < GameInfo.clonesAmount; i += 2)
        {
            Vector2 spawnPos;

            float yDist = 1.5f;
            RaycastHit2D rayToWall = Physics2D.Raycast(transform.position, Vector2.up * -i, Mathf.Infinity, LayerMask.GetMask("Wall", "GoalCol"));
            float distToWall = Mathf.Abs(rayToWall.point.y - transform.position.y);
            if (distToWall < 2f) yDist = distToWall - 0.5f;

            if (i < 2) spawnPos = new Vector2(transform.position.x, transform.position.y + -yDist * i);
            else spawnPos = new Vector2(transform.position.x + -yDist * i, transform.position.y);
            GameObject clone = Instantiate(gameObject, spawnPos, transform.rotation);

            PlayerManager manager = clone.GetComponent<PlayerManager>();
            manager.Setup(mainMovement.playerNumber);
            Destroy(clone.GetComponent<ClonesPowerup>());
            Destroy(clone.GetComponent<PlayerPowerupScript>());
            ClonesDeathScript deathScript = clone.AddComponent<ClonesDeathScript>();
            deathScript.powerupScript = this;
            manager.Die += deathScript.KillClone;
            clone.GetComponent<Rigidbody2D>().velocity = GetComponent<Rigidbody2D>().velocity;
            ReplayManager.AddReplayObjectToRecordScenes(clone.GetComponent<ReplayObject>());
            manager.isBot = true;
            clone.GetComponent<PlayerMovement>().isBot = true;
            clone.transform.localScale *= 0.7f;
            GameObject spawnEffect = Instantiate(manager.respawnEffectPrefab, clone.transform.position, Quaternion.identity);
            spawnEffect.GetComponentInChildren<Renderer>().material = manager.playerMat;
            ReplayManager.AddReplayObjectToRecordScenes(spawnEffect.GetComponent<ReplayObject>());
            clones.Add(clone.GetComponent<PlayerMovement>());
        }

        StartCoroutine(Use());
    }

    private IEnumerator Use()
    {
        float timer = GameInfo.clonesDuration;
        while (timer > 0f)
        {
            bool clonesBoost = mainMovement.BoostsAvailable() < boostsLastFrame;

            int mostBoosts = 0;
            foreach (PlayerMovement clone in clones)
            {
                clone.SetThrust(mainMovement.thrusting);
                clone.SetTurn(mainMovement.turnInputValue);
                if (clone.BoostsAvailable() > mostBoosts) mostBoosts = clone.BoostsAvailable();
            }
            foreach (PlayerMovement clone in clones)
            {
                if (clonesBoost)
                {
                    clone.UseBoost();
                    clone.ChangeBoostsAvailable(mostBoosts - 1, true);
                }
                else
                {
                    clone.ChangeBoostsAvailable(mostBoosts, true);
                }
            }

            boostsLastFrame = mostBoosts;
            if (clonesBoost) boostsLastFrame--;

            timer -= Time.deltaTime;
            yield return null;
        }
        endPowerup = true;
    }

    public override void Die()
    {
        base.Die();
    }

    public override void EndPowerup()
    {
        base.EndPowerup();
        for (int i = clones.Count - 1; i > 0; i--)
        {
            if (clones[i] != mainMovement)
            {
                clones[i].GetComponent<PlayerManager>().TriggerDie();
            }
        }
    }
}
