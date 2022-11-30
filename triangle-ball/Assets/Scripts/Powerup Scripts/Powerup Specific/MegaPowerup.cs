using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class MegaPowerup : Powerup
{
    private PlayerManager manager;
    private PlayerMovement m;
    private Animator anim;
    //private readonly float massIncrease = 2f;

    public override void UsePowerup()
    {
        manager = GetComponent<PlayerManager>();

        //transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        anim = GetComponent<Animator>();
        anim.SetBool("Mega", true);
        manager.canBeKilled = GameInfo.megaCanBeDemolished;

        if (GameInfo.megaCanBeDemolished)
            base.UsePowerup();
        else
            manager.canBeKilled = false;

        GetComponent<Rigidbody2D>().mass *= GameInfo.megaMassIncrease;
        m = GetComponent<PlayerMovement>();
        m.thrustForce *= GameInfo.megaMassIncrease;
        m.boostForce *= GameInfo.megaMassIncrease;

        GameObject effect = Instantiate(manager.respawnEffectPrefab, transform);
        effect.GetComponentInChildren<Renderer>().material = manager.playerMat;
        ReplayManager.AddReplayObjectToRecordScenes(effect.GetComponent<ReplayObject>());

        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {

        yield return new WaitForSeconds(GameInfo.megaDuration);
        endPowerup = true;
    }

    public override void EndPowerup()
    {
        if (GameInfo.megaCanBeDemolished)
            base.EndPowerup();
        else
            manager.canBeKilled = true;

        //transform.localScale = new Vector3(1f, 1f, 1f);
        anim.SetBool("Mega", false);

        GetComponent<Rigidbody2D>().mass /= GameInfo.megaMassIncrease;
        m.thrustForce /= GameInfo.megaMassIncrease;
        m.boostForce /= GameInfo.megaMassIncrease;

        GameObject effect = Instantiate(manager.respawnEffectPrefab, transform);
        effect.GetComponentInChildren<Renderer>().material = manager.playerMat;
        ReplayManager.AddReplayObjectToRecordScenes(effect.GetComponent<ReplayObject>());
    }
}
