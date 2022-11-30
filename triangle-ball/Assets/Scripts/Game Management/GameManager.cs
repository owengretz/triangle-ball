using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UltimateReplay;
using UltimateReplay.Storage;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public RectTransform ui;

    public bool straightKickoffOnly;
    public bool skipCountdown;
    [HideInInspector] public bool menuGame;

    //[HideInInspector] public bool gamePaused;
    public BGScrollerGameScript bgScroller;


    public static GameManager instance;

    [HideInInspector] public List<int> scoreValues = new List<int>();

    [HideInInspector] public readonly float afterGoalDelay = 3f;

    public enum Teams { Blue, Red, Green, None };
    public Dictionary<Teams, int> teamIndexes = new Dictionary<Teams, int>()
    {
        { Teams.Blue, 0 },
        { Teams.Red, 1 },
        { Teams.Green, 2 },
        { Teams.None, 0 }
    };
    public Dictionary<int, Teams> indexTeams = new Dictionary<int, Teams>()
    {
        { 0, Teams.Blue },
        { 1, Teams.Red },
        { 2, Teams.Green },
        { -1, Teams.Blue }
    };

    public Color[] colours;


    //public enum Mode { OneVsOne, OneVsOneVsOne, TwoVsTwo };
    //public Mode mode;

    public enum State { Countdown, Running, GoalScored, Replay, GameOver, Paused };
    public State state;

    private ModeManager mm;
    private TimeManager tm;
    private InstantReplay ir;


    //[HideInInspector] public int numPlayers;
    [HideInInspector] public int winnerIndex;
    public GameObject playerPrefab;
    public List<PlayerManager> players = new List<PlayerManager>();
    //public List<bool> areBots = new List<bool>();
    public GameObject ballPrefab;

    public Material[] playerMaterials;

    public ReplayScene recordScene;
    [HideInInspector] public ReplayHandle instantReplay;

    public GameObject cameraPrefab;
    private EZCameraShake.CameraShaker cam;

    private int lastKickoffStrat = 0;

    //[HideInInspector] public ReplayFileTarget fullReplayStorage = null;
    //[HideInInspector] public ReplayHandle fullReplay;

    //private string mapName;
    //private Scene map;

    public bool botTest;

    public static event Action<Teams, Teams> OnGoalScore;
    public static void TriggerGoalScored(Teams scorer, Teams scoredAgainst)
    {
        if (OnGoalScore != null)
            OnGoalScore(scorer, scoredAgainst);
    }

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;


        instance = this;

        TutorialManager tutorial = FindObjectOfType<TutorialManager>();
        if (GameInfo.tutorial)
        {
            tutorial.StartTutorial();
            return;
        }
        else
        {
            Destroy(tutorial.gameObject);
        }

        if (botTest)
            return;

        if (GameInfo.numberOfPlayers == 3) mm = gameObject.AddComponent<ThreePlayerManager>();
        else mm = gameObject.AddComponent<EvenPlayerManager>();
        mm.Setup();

        tm = GetComponent<TimeManager>();

        ir = gameObject.AddComponent<InstantReplay>();

        if (GameInfo.numberOfPlayers == 3) playerMaterials[2].color = colours[2];
        else playerMaterials[2].color = colours[3];
        //if (mode == Mode.OneVsOne) numPlayers = 2;
        //else if (mode == Mode.OneVsOneVsOne)
        //{
        //    numPlayers = 3;
        //    playerMaterials[2].color = colours[2];
        //}
        //else if (mode == Mode.TwoVsTwo)
        //{
        //    numPlayers = 4;
        //    playerMaterials[2].color = colours[3];
        //    playerMaterials[3].color = colours[4];
        //}

        for (int i = 0; i < 3; i++)
        {
            scoreValues.Add(0);
        }

        if (GameInfo.powerupsOn)
        {
            GetComponent<PowerupManager>().Setup();
        }
        else Destroy(GetComponent<PowerupManager>());
    }

    private IEnumerator Start()
    {
        var op = SceneManager.LoadSceneAsync(GameInfo.mapInfo.scene, LoadSceneMode.Additive);
        yield return new WaitUntil(() => op.isDone);

        if (GameInfo.tutorial)
            yield break;

        OnGoalScore += GoalScored;

        DeleteCameras();

        if (botTest)
        {
            Instantiate(ballPrefab);

            SpawnPlayers();

            for (int i = 0; i < players.Count; i++)
            {
                players[i].GetComponent<SpriteRenderer>().material = playerMaterials[i];
            }
            players[0].transform.position = Vector2.left * 100f;
            players[1].transform.position = Vector2.right * 100f;

            SetPlayerMovement(true);

            //FindObjectOfType<BotTestManager>().SetUpTestSituation();

            yield return new WaitForSeconds(1f);

            for (int i = 0; i < players.Count; i++)
            {
                players[i].gameObject.SetActive(true);
            }

            FindObjectOfType<BotTestManager>().SetUpTestSituation();

            yield break;
        }

        bgScroller.Setup();

        //map = SceneManager.GetSceneByName(GameInfo.mapInfo.scene);

        //recordScene = ReplayScene.FromScene(map);
        //fullReplayStorage = ReplayFileTarget.CreateUniqueReplayFile("ReplayFiles");
        //fullReplayStorage = ReplayFileTarget.CreateReplayFile("ReplayFiles/test_replay.replay");
        //fullReplay = ReplayManager.BeginRecording(fullReplayStorage, recordScene, allowEmptyScene: true);

        StartRound();
    }

    private void StartRound()
    {
        state = State.Countdown;

        recordScene = ReplayScene.FromScene(SceneManager.GetSceneByName(GameInfo.mapInfo.scene));
        instantReplay = ReplayManager.BeginRecording(null, recordScene);

        ReplayManager.AddReplayObjectToRecordScenes(Instantiate(ballPrefab).GetComponent<ReplayObject>());

        SpawnCamera();

        SpawnPlayers();

        mm.SetColours();

        mm.SetKickoffPositions();

        tm.ResetTimeSinceKickoff();

        if (skipCountdown) tm.ActivateGame();
        else tm.StartCountdown();
    }
    
    public void SetPlayerMovement(bool canMove)
    {
        foreach (PlayerManager player in players) player.SetCanMove(canMove); 
    }

    public void GoalScored(Teams scorer, Teams scoredAgainst)
    {
        if (botTest) return;

        tm.vignetteAnim.SetBool("Show", false); // meh

        bool disableGoalResetCondition = GameInfo.disableGoalReset && !tm.overtime && tm.clockValue != 0f;

        if (!disableGoalResetCondition) state = State.GoalScored;

        if (scorer == scoredAgainst) for (int i = 0; i < scoreValues.Count; i++) if (indexTeams[i] != scorer) scoreValues[i]++;

        if (scorer != scoredAgainst) scoreValues[teamIndexes[scorer]]++;

        UIManager.instance.UpdateScoreText();

        BallManager ballManager = FindObjectOfType<BallManager>();

        float[] timeOfLastTouches = ballManager.timeOfLastTouches;
        mm.GoalScored(scorer, ballManager.gameObject);

        if (disableGoalResetCondition)
        {
            Transform ball = Instantiate(ballPrefab).transform;
            ReplayManager.AddReplayObjectToRecordScenes(ball.GetComponent<ReplayObject>());
            BotUtils[] bots = FindObjectsOfType<BotUtils>();
            foreach (BotUtils bot in bots)
                bot.ball = ball;

            return;
        }

        StartCoroutine(EndRound(scorer, timeOfLastTouches));
    }

    public Teams TeamOfScorer(BallManager b, Teams scoredAgainst)
    {
        if (b.colourOfLastTouches[0] != Teams.None && b.colourOfLastTouches[0] != scoredAgainst) return b.colourOfLastTouches[0];
        else if (b.colourOfLastTouches[1] != Teams.None && b.colourOfLastTouches[1] != scoredAgainst) return b.colourOfLastTouches[1];
        else return scoredAgainst;

        //Teams team;
        //if (b.colourOfLastTouches[0] != Teams.none && b.colourOfLastTouches[0] != scoredAgainst) team = b.colourOfLastTouches[0];
        //else if (b.colourOfLastTouches[1] != Teams.none && b.colourOfLastTouches[1] != scoredAgainst) team = b.colourOfLastTouches[1];
        //else team = scoredAgainst;
        ////else team = Teams.none;
        ////Debug.Log("last touch: " + b.colourOfLastTouches[0] + "\t 2nd last touch: " + b.colourOfLastTouches[1] + "\t scorer: " + team);
        //return team;
        ////return b.playerMats[teamIndexes[team]].color;
    }

    public IEnumerator EndRound(Teams scorer, float[] timeOfLastTouches)
    {
        yield return new WaitForSeconds(afterGoalDelay);

        players.Clear();

        DestroyOnResetReplayObjects();

        ReplayManager.StopRecording(ref instantReplay);

        ReplayManager.RemoveReplayObjectFromRecordScenes(cam.GetComponent<ReplayObject>());

        if (menuGame)
        {
            OnReplayEnd();
        }
        else
        {
            instantReplay = ir.Play(scorer, timeOfLastTouches);
            state = State.Replay;
        }
    }

    public void OnReplayEnd()
    {
        if (GameInfo.tutorial) return;

        int highestScore = scoreValues.Max();
        int amountOfTeamsAtHighest = 0;
        foreach (int score in scoreValues) if (score == highestScore) amountOfTeamsAtHighest++;

        // if it's still normal game restart round
        if ((tm.clockValue >= 1f && !TimeManager.instance.overtime && highestScore < GameInfo.maxScore)
            || (GameInfo.matchLength == -1f && highestScore < GameInfo.maxScore))
        {
            StartRound();
        }
        else
        {
            // going to overtime
            if (amountOfTeamsAtHighest > 1)
            {
                BallManager ball = FindObjectOfType<BallManager>();

                // if it goes to overtime from the ball touching wall
                if (ball != null)
                {
                    ReplayManager.StopRecording(ref instantReplay);
                    foreach (PlayerManager player in players) player.SetCanMove(false);
                    ball.Explode(ball.colourOfLastTouches[0]);
                    StartCoroutine(DelayStartRound());
                }
                // if it's overtime and there was a goal but it's still a tie
                else
                {
                    StartRound();
                }
            }
            // game over
            else
            {
                ReplayManager.StopRecording(ref instantReplay);
                foreach (PlayerManager player in players) player.SetCanMove(false);
                BallManager ball = FindObjectOfType<BallManager>();

                // if game ends from ball touching wall
                if (ball != null && !TimeManager.instance.overtime)
                {
                    ball.Explode(indexTeams[scoreValues.IndexOf(scoreValues.Max())]);
                    StartCoroutine(DelayEndGame());
                }
                // if game ends from someone scoring in overtime
                else
                {
                    EndGame();
                }
            }
        }
    }
    private IEnumerator DelayStartRound()
    {
        yield return new WaitForSeconds(2f);
        players.Clear();
        DestroyOnResetReplayObjects();
        foreach (BoostPad boost in FindObjectsOfType<BoostPad>()) if (!boost.up) boost.ChangeState(true);

        TimeManager.instance.overtime = true;
        StartRound();
    }
    private IEnumerator DelayEndGame()
    {
        yield return new WaitForSeconds(2f);
        EndGame();
    }


    public void DeleteCameras()
    {
        foreach (Camera camera in FindObjectsOfType<Camera>())
        {
            Destroy(camera.gameObject);
        }
    }

    public void SpawnCamera()
    {
        cam = Instantiate(cameraPrefab.GetComponent<EZCameraShake.CameraShaker>());
        Camera camera = cam.GetComponentInChildren<Camera>();
        ReplayManager.AddReplayObjectToRecordScenes(cam.GetComponent<ReplayObject>());
        if (menuGame)
        {
            camera.orthographicSize = 12f;
            camera.transform.position = new Vector3(-7.7f, 0f, -10f);
        }
        else
        {
            camera.orthographicSize = GameInfo.mapInfo.cameraZoom;
        }
    }

    private void SpawnPlayers()
    {
        List<Teams> teams = new List<Teams>();
        for (int i = 0; i < GameInfo.numberOfPlayers; i++)
        {
            if (GameInfo.numberOfPlayers != 3)
            {
                teams.Add(i % 2 == 0 ? Teams.Blue : Teams.Red);
            }
            else
            {
                teams.Add(indexTeams[i]);
            }
        }
        

        for (int i = 0; i < GameInfo.numberOfPlayers; i++)
        {
            PlayerManager player = Instantiate(playerPrefab).GetComponent<PlayerManager>();
            ReplayManager.AddReplayObjectToRecordScenes(player.GetComponent<ReplayObject>());

            player.team = teams[i];
            players.Add(player);
        }

        for (int i = 0; i < players.Count; i++)
        {
            if (GameInfo.botPlayers[i] != 0) players[i].isBot = true;

            players[i].Setup(i);
        }

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].isBot)
                players[i].GetComponent<BotUtils>().Setup(teams[i]);
        }
    }


    public void EndGame(bool goingToMenu = false)
    {
        cam = Instantiate(cameraPrefab.GetComponent<EZCameraShake.CameraShaker>());

        state = State.GameOver;

        tm.vignetteAnim.SetBool("Show", false);

        winnerIndex = scoreValues.IndexOf(scoreValues.Max());
        
        if (!goingToMenu)
        {
            Transitions.Function func = UIManager.instance.EndGame;
            Transitions.instance.Fade(func);
        }

        //DestroyOnResetReplayObjects();

        //MapObject[] mapObjects = FindObjectsOfType<MapObject>();
        //foreach (MapObject obj in mapObjects) obj.Hide();

        //ReplayManager.StopRecording(ref fullReplay);
        //fullReplayStorage.Dispose();

        //string[] replayFiles = Directory.GetFiles("ReplayFiles");
        //fullReplayStorage = ReplayFileTarget.ReadReplayFile(replayFiles[0]);

        //fullReplay = ReplayManager.BeginPlayback(replaySource: fullReplayStorage, allowEmptyScene: true);

        
    }



    private void DestroyOnResetReplayObjects()
    {
        foreach (ReplayObject replayObj in FindObjectsOfType<ReplayObject>())
        {
            if (replayObj.destroyOnReset)
            {
                Destroy(replayObj.gameObject);
                ReplayManager.RemoveReplayObjectFromRecordScenes(replayObj);
            }
        }
    }




    public int GetBotKickoffStrat()
    {
        int strat = UnityEngine.Random.Range(1, 6);
        int numBotPlayers = 0;
        foreach (int bot in GameInfo.botPlayers) if (bot != 0) numBotPlayers++;
        if (numBotPlayers > 1 && strat == lastKickoffStrat) strat = GetBotKickoffStrat();
        lastKickoffStrat = strat;
        return strat;
    }

    public void ShakeCamera(float intensity, float duration)
    {
        // if (settings.cameraShakeOn)
        if (!menuGame)
            cam.ShakeOnce(intensity, GameInfo.shakeRoughness, duration, duration);
    }


    private void OnDestroy()
    {
        OnGoalScore -= GoalScored;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.G))
    //    {
    //        cam.transform.position = Vector3.right;

    //        ui.transform.position = Vector2.left;
    //    }
    //    else if (Input.GetKeyDown(KeyCode.H))
    //    {
    //        cam.transform.position = Vector3.zero;

    //        ui.localPosition = ui.worldToLocalMatrix * Vector3.zero;
    //    }
    //}

    //public Vector2 WorldToRect(Vector2 coord)
    //{
    //    //then you calculate the position of the UI element
    //    //0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.

    //    Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(coord);
    //    Vector2 WorldObject_ScreenPosition = new Vector2(
    //    ((ViewportPosition.x * ui.sizeDelta.x) - (ui.sizeDelta.x * 0.5f)),
    //    ((ViewportPosition.y * ui.sizeDelta.y) - (ui.sizeDelta.y * 0.5f)));

    //    //now you can set the position of the ui element
    //    //ui.anchoredPosition = WorldObject_ScreenPosition;

    //    Debug.Log(WorldObject_ScreenPosition);

    //    return WorldObject_ScreenPosition;

    //    //Vector3 camPos = Camera.main.transform.position;

    //    //float pos = ui.rect.width / 1f;

    //    //Debug.Log(Camera.main.ViewportToWorldPoint(Vector2.zero));

    //    //return coord * pos;
    //}
}
