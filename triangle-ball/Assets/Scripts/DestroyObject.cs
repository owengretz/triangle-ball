using System.Collections;
using UltimateReplay;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);

        if (gameObject != null)
        {
            // destroy particle after 2 seconds
            Destroy(gameObject);
            
            if (GameManager.instance != null)
                ReplayManager.RemoveReplayObjectFromRecordScenes(GetComponent<ReplayObject>());
        }
    }


}
