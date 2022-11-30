using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class PentagonPowerup : Powerup
{
    private PlayerManager manager;

    private PolygonCollider2D newCol;
    private PolygonCollider2D[] newDemos = new PolygonCollider2D[2];


    public override void UsePowerup()
    {
        newCol = gameObject.AddComponent<PolygonCollider2D>();
        newCol.points = new Vector2[5] { new Vector2(0f, 0.34f), new Vector2(-0.79f, 0.72f), new Vector2(-0.425f, -0.23f), 
                                         new Vector2(0.425f, -0.23f), new Vector2(0.79f, 0.72f) };

        for (int i = 0; i < 2; i++)
        {
            newDemos[i] = transform.GetChild(0).gameObject.AddComponent<PolygonCollider2D>();
            newDemos[i].isTrigger = true;
            newDemos[i].offset = new Vector2(0f, -0.65f);
        }
        newDemos[0].points = new Vector2[4] { new Vector2(0.827f, 0.716f), new Vector2(0.793f, 0.751f), new Vector2(0.724f, 0.683f),
                                         new Vector2(0.759f, 0.648f) };
        newDemos[1].points = new Vector2[4] { new Vector2(-0.827f, 0.716f), new Vector2(-0.793f, 0.751f), new Vector2(-0.724f, 0.683f),
                                         new Vector2(-0.759f, 0.648f) };


        manager = GetComponent<PlayerManager>();
        manager.anim.SetBool("Pentagon", true);

        if (GameInfo.pentagonCanBeDemolished)
            base.UsePowerup();
        else
            manager.canBeKilled = false;

        GameObject effect = Instantiate(manager.respawnEffectPrefab, transform);
        effect.GetComponentInChildren<Renderer>().material = manager.playerMat;
        ReplayManager.AddReplayObjectToRecordScenes(effect.GetComponent<ReplayObject>());

        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {

        yield return new WaitForSeconds(GameInfo.pentagonDuration);
        endPowerup = true;
    }

    public override void EndPowerup()
    {
        if (GameInfo.pentagonCanBeDemolished)
            base.EndPowerup();
        else
            manager.canBeKilled = true;

        Destroy(newCol);
        foreach (PolygonCollider2D trig in newDemos) Destroy(trig);

        manager.anim.SetBool("Pentagon", false);

        if (manager.anim.GetBool("Dead")) 
            return;

        GameObject effect = Instantiate(manager.respawnEffectPrefab, transform);
        effect.GetComponentInChildren<Renderer>().material = manager.playerMat;
        ReplayManager.AddReplayObjectToRecordScenes(effect.GetComponent<ReplayObject>());
    }




    public Vector2 BumpDir(Vector2 colPoint)
    {
        float side = Mathf.Sign(Vector2.SignedAngle(transform.up, colPoint - (Vector2)transform.position));

        return (transform.up + transform.right * -side).normalized;
    }
}
