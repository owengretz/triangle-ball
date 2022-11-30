using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UltimateReplay.Storage;
using UnityEngine;

public class ReplayTest : MonoBehaviour
{
    private ReplayScene recordScene;
    private ReplayHandle handle;

    //private IEnumerator Start()
    //{
    //    recordScene = ReplayScene.FromCurrentScene();
    //    handle = ReplayManager.BeginRecording(null, recordScene);

    //    yield return new WaitForSeconds(5f);

    //    ReplayManager.StopRecording(ref handle);
    //    ReplayManager.BeginPlayback();
    //}

    private void Start()
    {
        ReplayFileTarget fullReplayStorage = ReplayFileTarget.ReadReplayFile("ReplayFiles/test_replay.replay");

        ReplayManager.BeginPlayback(replaySource: fullReplayStorage, allowEmptyScene: true);
    }
}
