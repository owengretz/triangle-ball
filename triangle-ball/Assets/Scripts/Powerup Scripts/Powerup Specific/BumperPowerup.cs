using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class BumperPowerup : Powerup
{
    //private readonly float powerupDuration = 8f;
    //private readonly float launchVelocity = 15f;

    public override void UsePowerup()
    {
        base.UsePowerup();

        Vector2 spawnPos = transform.position + transform.up * 1.3f;

        GameObject bumper = Instantiate(PowerupManager.instance.bumper, spawnPos, Quaternion.identity);
        ReplayManager.AddReplayObjectToRecordScenes(bumper.GetComponent<ReplayObject>());
        bumper.GetComponent<Rigidbody2D>().velocity = transform.up * GameInfo.bumperLaunchSpeed;
        bumper.GetComponent<BumperScript>().duration = GameInfo.bumperDuration;

        // so that the cooldown for next powerup starts right away
        // this way the fact that the bumper lasts a while is a buff not a nerf
        endPowerup = true;
    }

    public override void Die()
    {
        
    }
}
