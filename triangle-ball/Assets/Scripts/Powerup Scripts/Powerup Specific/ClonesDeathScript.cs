using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class ClonesDeathScript : MonoBehaviour
{
    public ClonesPowerup powerupScript;

    public void KillClone()
    {
        GameObject demoExplosion = Instantiate(GetComponent<PlayerManager>().demoExplosionPrefab, transform.position, Quaternion.identity);
        ReplayManager.AddReplayObjectToRecordScenes(demoExplosion.GetComponent<ReplayObject>());

        powerupScript.clones.Remove(GetComponent<PlayerMovement>());
        ReplayManager.RemoveReplayObjectFromRecordScenes(gameObject.GetComponent<ReplayObject>());
        Destroy(gameObject);
    }
}
