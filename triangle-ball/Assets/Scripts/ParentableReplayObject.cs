using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class ParentableReplayObject : MonoBehaviour
{
    private void OnDisable()
    {
        ReplayManager.RemoveReplayObjectFromRecordScenes(GetComponent<ReplayObject>());
    }
}
