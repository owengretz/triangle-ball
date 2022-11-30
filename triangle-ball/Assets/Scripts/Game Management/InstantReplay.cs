using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class InstantReplay : MonoBehaviour
{
    private readonly float maxReplayDuration = 12f;
    private readonly float prefReplayDuration = 5f;
    private readonly float slowmoDuration = 0.75f;
    private readonly float slowmoTimeModifier = 0.3f;

    private bool skipReplay;

    private MapObject[] mapObjects;
    private UIManager ui;
    private GameObject powerupCanvas;

    public ReplayHandle Play(GameManager.Teams scorer, float[] timeOfLastTouches)
    {
        // hide stuff like boost or else replay will destroy them
        mapObjects = FindObjectsOfType<MapObject>();
        foreach (MapObject obj in mapObjects) obj.Hide();

        // hide powerup ui for replay
        if (GameInfo.powerupsOn)
        {
            powerupCanvas = GameObject.Find("Powerup Canvas(Clone)");
            powerupCanvas.SetActive(false);
        }

        ReplayHandle handle = ReplayManager.BeginPlayback(allowEmptyScene: true);

        StartCoroutine(PlayReplay(handle, scorer, timeOfLastTouches));

        return handle;
    }

    public IEnumerator PlayReplay(ReplayHandle handle, GameManager.Teams scorer, float[] timeOfLastTouches)
    {
        ui = FindObjectOfType<UIManager>();
        ui.anim.SetBool("Replay", true);

        float timeSinceKickoff = TimeManager.instance.ElapsedTimeSinceKickoff;

        //Debug.Log(string.Format("time since kickoff: {0:.##}s", timeSinceKickoff));

        float timeOfLastTouch = scorer switch
        {
            GameManager.Teams.Blue => timeOfLastTouches[0],
            GameManager.Teams.Red => timeOfLastTouches[1],
            GameManager.Teams.Green => timeOfLastTouches[2],
            GameManager.Teams.None => 0f,
            _ => 0f,
        };

        float timeSinceLastTouch = timeSinceKickoff - timeOfLastTouch ;

        float replayDuration = timeSinceKickoff;
        // if game has gone on longer than we want replay length then we check:
        //      if the last touch from the scoring team was longer than the max replay duration, cut it off at the max duration
        //      if it less than the max but greater than the preferred length, set it to the time of the last touch (+ 2 seconds for a lead up to the hit)
        //      if it was shorter then we just leave as the preferred length
        if (replayDuration > prefReplayDuration)
        {
            if (timeSinceLastTouch > maxReplayDuration)
            {
                replayDuration = maxReplayDuration;
            }
            else if (timeSinceLastTouch > prefReplayDuration)
            {
                replayDuration = timeSinceLastTouch + 2f;
            }
            else
            {
                replayDuration = prefReplayDuration;
            }
        }

        // account for time after the ball is scored
        replayDuration += GameManager.instance.afterGoalDelay;


        ReplayManager.SetPlaybackTime(handle, replayDuration, PlaybackOrigin.End);


        skipReplay = false;
        foreach (UnityEngine.UI.Image indicator in ui.playerIndicators) indicator.gameObject.SetActive(true);
        StartCoroutine(CheckSkipReplay(handle));


        float timer = 0f;
        float timeOfSlowmo = replayDuration - (timeSinceLastTouch + (slowmoDuration / 2f)) - GameManager.instance.afterGoalDelay;

        //Debug.Log(string.Format("time of slowmo: {0:.##}s in, {1:.##} to go", timeOfSlowmo, replayDuration - timeOfSlowmo));

        while (timer < replayDuration && !skipReplay)
        {
            if (GameManager.instance.state != GameManager.State.Paused)
            {
                if (timer > timeOfSlowmo && timer < timeOfSlowmo + slowmoDuration)
                {
                    Time.timeScale = slowmoTimeModifier;
                }
                else
                {
                    Time.timeScale = 1f;
                }
                timer += Time.deltaTime;
            }
            yield return null;
        }

        if (GameManager.instance.state != GameManager.State.Paused)
        {
            Time.timeScale = 1f;
        }

        if (ReplayManager.IsReplaying(handle))
        {
            ReplayManager.StopPlaybackDelayed(handle);

            while (ReplayManager.IsReplaying(handle)) 
            {
                yield return null;
            }
        }

        EndOfReplay();
        GameManager.instance.OnReplayEnd();
    }

    private void EndOfReplay()
    {
        foreach (MapObject obj in mapObjects) obj.Show();
        ui.anim.SetBool("Replay", false);

        if (GameInfo.powerupsOn)
        {
            powerupCanvas.SetActive(true);
        }
    }



    private IEnumerator CheckSkipReplay(ReplayHandle handle)
    {
        List<GameObject> indicators = new List<GameObject>();
        List<bool> skipped = new List<bool>();

        foreach (UnityEngine.UI.Image indicator in ui.playerIndicators)
        {
            indicators.Add(indicator.gameObject);
            skipped.Add(false);
        }

        for (int i = 0; i < indicators.Count; i++)
        {
            if (i > GameInfo.numberOfPlayers - 1 || GameInfo.botPlayers[i] != 0/* && i != 0*/)
            {
                ui.playerIndicators[i].gameObject.SetActive(false);
                skipped[i] = true;
            }
        }

        bool allBots = true;
        foreach (bool check in skipped) if (check == false) allBots = false;
        if (allBots)
        {
            ui.playerIndicators[0].gameObject.SetActive(true);
            skipped[0] = false;
        }

        int players = 0;
        foreach (bool check in skipped) if (check == true) players++;
        ui.playerIndicatorHolder.anchoredPosition = new Vector2(ui.playerIndicatorHolder.anchoredPosition.x, (-125f / 3f) * players);

        while (ReplayManager.IsReplaying(handle) && !skipReplay)
        {
            yield return null;

            for (int i = 0; i < indicators.Count; i++)
            {
                if (skipped[i] == false)
                {
                    if (/*Mathf.Abs(Input.GetAxis("Horizontal" + i)) > 0.1f || */Sinput.GetButtonDown("Player " + (i + 1) + " Boost") || Sinput.GetButtonDown("Player " + (i + 1) + " Thrust"))
                    {
                        skipped[i] = true;
                        ui.playerIndicators[i].gameObject.SetActive(false);
                    }
                }
            }

            int playersNotReady = 0;

            foreach (bool check in skipped) if (check == false) playersNotReady++;

            if (playersNotReady == 0) skipReplay = true;
        }
    }
}
