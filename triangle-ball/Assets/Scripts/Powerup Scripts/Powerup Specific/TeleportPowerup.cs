using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class TeleportPowerup : Powerup
{
    private PlayerManager me;
    private PlayerManager them;

    public override void UsePowerup()
    {
        base.UsePowerup();

        //duration = 0.5f;

        PlayerManager closestOpponent = null;
        float closestRotation = 99999f;

        // gets opponent closest to where we're pointing
        foreach (PlayerManager player in FindObjectsOfType<PlayerManager>())
        {
            if (player.team != GetComponent<PlayerManager>().team && !player.isDead)
            {
                Vector2 difference = player.transform.position - transform.position;
                float rotation = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg - 90f;

                Quaternion q = transform.rotation;
                float siny_cosp = 2 * (q.w * q.z + q.x * q.y);
                float cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
                float zRotation = Mathf.Atan2(siny_cosp, cosy_cosp) * Mathf.Rad2Deg;

                float rotationDifference = Mathf.Abs(zRotation - rotation);
                if (rotationDifference > 180f) rotationDifference = Mathf.Abs(rotationDifference - 360f);
                

                if (rotationDifference < closestRotation)
                {
                    closestRotation = rotationDifference;
                    closestOpponent = player;
                }
            }
        } 

        me = GetComponent<PlayerManager>();
        GameObject effect = Instantiate(me.respawnEffectPrefab, transform);
        effect.GetComponentInChildren<Renderer>().material = me.playerMat;
        ReplayManager.AddReplayObjectToRecordScenes(effect.GetComponent<ReplayObject>());

        if (closestOpponent != null) them = closestOpponent;
        else
        {
            endPowerup = true;
            return;
        }


        effect = Instantiate(them.respawnEffectPrefab, them.transform);
        effect.GetComponentInChildren<Renderer>().material = them.playerMat;
        ReplayManager.AddReplayObjectToRecordScenes(effect.GetComponent<ReplayObject>());

        StartCoroutine(TeleportDelay());
    }

    // slight delay before teleporting
    private IEnumerator TeleportDelay()
    {
        yield return new WaitForSeconds(GameInfo.teleportDelay);

        endPowerup = true;
    }

    public override void EndPowerup()
    {
        base.EndPowerup();

        if (them == null) return;
        if (them.isDead) return;
        if (them.GetComponent<SpikesPowerup>() != null)
        {
            them.GetComponent<SpikesPowerup>().endPowerup = true;
            them.GetComponent<SpikesPowerup>().EndPowerup();
        }

        // store position, rotation & velocity
        Vector2 myPos = transform.position;
        Vector2 theirPos = them.transform.position;
        Quaternion myRot = transform.rotation;
        Quaternion theirRot = them.transform.rotation;
        Vector2 myVel = GetComponent<Rigidbody2D>().velocity;
        Vector2 theirVel = them.GetComponent<Rigidbody2D>().velocity;

        // swap
        transform.position = theirPos;
        transform.rotation = theirRot;
        GetComponent<Rigidbody2D>().velocity = theirVel;
        them.transform.position = myPos;
        them.transform.rotation = myRot;
        them.GetComponent<Rigidbody2D>().velocity = myVel;

        // particle effect
        GameObject effect = Instantiate(me.respawnEffectPrefab, transform);
        effect.GetComponentInChildren<Renderer>().material = me.playerMat;
        ReplayManager.AddReplayObjectToRecordScenes(effect.GetComponent<ReplayObject>());
        effect = Instantiate(them.respawnEffectPrefab, them.transform);
        effect.GetComponentInChildren<Renderer>().material = them.playerMat;
        ReplayManager.AddReplayObjectToRecordScenes(effect.GetComponent<ReplayObject>());
    }
}
