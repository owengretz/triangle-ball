using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public CanvasGroup uiGroup;
    public Sprite[] artSprites; // smiling, pointing, thumbs up, annoyed
    public Image triangleImage;
    public TMP_Text text;
    public Animator anim;
    public Animator pressToContAnim;

    public GameObject targetPrefab;

    private PlayerManager player;
    private Transform ball;
    private Material[] mats;

    private BoostPad[] boostPads;

    private ScriptLine[] script;

    private int lineIndex;

    private bool scoredOnce;

    public void StartTutorial()
    {
        GameManager.instance.DeleteCameras();
        GameManager.instance.SpawnCamera();
        TimeManager.instance.clockValue = 180f;
        UIManager.instance.scoreboard.SetActive(false);

        mats = GameManager.instance.playerMaterials;

        uiGroup.alpha = 1f;

        script = GetScript();
        lineIndex = 0;

        StartCoroutine(LessonOne());

        //StartCoroutine(Skip());
    }

    private IEnumerator Skip()
    {
        player = Instantiate(GameManager.instance.playerPrefab).GetComponent<PlayerManager>();
        player.team = GameManager.Teams.Blue;
        player.Setup(0);
        player.GetComponent<SpriteRenderer>().material = mats[0];

        player.m.ChangeBoostsAvailable(0, true);
        player.SetTransform(Vector2.zero, 0f);
        player.SetCanMove(false);
        lineIndex = 7;


        while (FindObjectsOfType<BoostPad>().Length == 0) yield return null;
        boostPads = FindObjectsOfType<BoostPad>();
        //foreach (BoostPad boost in boostPads) boost.gameObject.SetActive(false);
        StartCoroutine(LessonThree());
    }


    private IEnumerator LessonOne()
    {
        while (FindObjectsOfType<BoostPad>().Length == 0) yield return null;
        boostPads = FindObjectsOfType<BoostPad>();
        foreach (BoostPad boost in boostPads) boost.gameObject.SetActive(false);

        player = Instantiate(GameManager.instance.playerPrefab).GetComponent<PlayerManager>();
        player.team = GameManager.Teams.Blue;
        player.Setup(0);
        player.GetComponent<SpriteRenderer>().material = mats[0];

        player.m.ChangeBoostsAvailable(0, true);
        player.SetTransform(Vector2.zero, 0f);
        player.SetCanMove(false);

        Vector2[] targetPositions = new Vector2[] { new Vector2(6.5f, 3f), new Vector2(-6.5f, -3f), new Vector2(6.5f, -3f), new Vector2(-6.5f, 3f), Vector2.zero };


        yield return StartCoroutine(Wait());

        yield return StartCoroutine(Wait());

        // show check points
        Transform targetTrans = Instantiate(targetPrefab).transform;
        TutorialTarget targetScript = targetTrans.GetComponent<TutorialTarget>();

        targetTrans.position = targetPositions[0];

        yield return StartCoroutine(Wait());

        yield return StartCoroutine(GiveControl());

        for (int i = 0; i < targetPositions.Length; i++)
        {
            targetTrans.position = targetPositions[i];
            targetScript.targetReached = false;

            while (!targetScript.targetReached)
            {
                yield return null;
            }
        }

        Destroy(targetTrans.gameObject);

        Transitions.instance.Fade(CallLessonTwo);
    }

    private void CallLessonTwo() { StartCoroutine(LessonTwo()); }
    private IEnumerator LessonTwo()
    {
        uiGroup.alpha = 1f;

        ResetPlayer(andBall: true);
        player.SetTransform(new Vector2(-7f, 0f), -90f);

        yield return StartCoroutine(Wait(true));

        yield return StartCoroutine(Wait());

        yield return StartCoroutine(GiveControl());

        GameManager.OnGoalScore += CallEndLessonTwo;
    }
    private void CallEndLessonTwo(GameManager.Teams scorer, GameManager.Teams scoredAgainst) { StartCoroutine(EndLessonTwo(scorer, scoredAgainst)); }
    private void CallEndLessonTwoPartTwo(GameManager.Teams scorer, GameManager.Teams scoredAgainst) { StartCoroutine(EndLessonTwo(scorer, scoredAgainst, false)); }
    private IEnumerator EndLessonTwo(GameManager.Teams scorer, GameManager.Teams scoredAgainst, bool firstTime = true)
    {
        ball.GetComponent<BallManager>().Explode(scorer);

        yield return new WaitForSeconds(2f);

        if (scoredAgainst == GameManager.Teams.Red)
        {
            if (firstTime)
            {
                scoredOnce = true;
                lineIndex = 6;
                GameManager.OnGoalScore -= CallEndLessonTwo;
                GameManager.OnGoalScore += CallEndLessonTwoPartTwo;
                Transitions.instance.Fade(CallLessonTwoPartTwo);
            }
            else
            {
                lineIndex = 7;
                GameManager.OnGoalScore -= CallEndLessonTwoPartTwo;
                Transitions.instance.Fade(CallLessonThree);
            }
        }
        else
        {
            Transitions.instance.Fade(CallWrongNet);
        }
    }
    private void CallWrongNet() { StartCoroutine(WrongNet()); }
    private IEnumerator WrongNet()
    {
        uiGroup.alpha = 1f;

        ResetPlayer(andBall: true);
        if (!scoredOnce)
        {
            player.SetTransform(new Vector2(-7f, 0f), -90f);
        }
        else
        {
            player.SetTransform(new Vector2(-9.5f, 0f), -25f);
            ball.position = new Vector2(-8f, 3f);
        }

        lineIndex = 5;
        yield return StartCoroutine(Wait(true));

        yield return StartCoroutine(GiveControl());
    }
    private void CallLessonTwoPartTwo() { StartCoroutine(LessonTwoPartTwo()); }
    private IEnumerator LessonTwoPartTwo()
    {
        uiGroup.alpha = 1f;

        ResetPlayer(andBall: true);
        player.SetTransform(new Vector2(-9.5f, 0f), -25f);
        ball.position = new Vector2(-8f, 3f);

        yield return StartCoroutine(Wait(true));

        StartCoroutine(GiveControl());
    }


    private void CallLessonThree() { StartCoroutine(LessonThree()); }
    private IEnumerator LessonThree()
    {
        uiGroup.alpha = 1f;
        foreach (BoostPad boost in boostPads) boost.gameObject.SetActive(true);

        ResetPlayer(andBall: false);

        yield return StartCoroutine(Wait(true));

        yield return StartCoroutine(Wait());

        yield return StartCoroutine(Wait());

        StartCoroutine(GiveControl());

        int boostAmount = 0;
        while (true)
        {
            if (player.m.BoostsAvailable() < boostAmount)
            {
                break;
            }

            boostAmount = player.m.BoostsAvailable();
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        Transitions.instance.Fade(CallLessonThreePartTwo);
    }
    private void CallLessonThreePartTwo() { StartCoroutine(LessonThreePartTwo()); }
    private IEnumerator LessonThreePartTwo()
    {
        uiGroup.alpha = 1f;

        GameInfo.playerDemolishThreshold = 1f;

        ResetPlayer(andBall: false);
        player.SetTransform(new Vector2(-3f, 0f), -90f);
        player.m.ChangeBoostsAvailable(1, true);

        PlayerManager dummy = Instantiate(GameManager.instance.playerPrefab).GetComponent<PlayerManager>();
        dummy.team = GameManager.Teams.Red;
        dummy.Setup(1);
        dummy.GetComponent<SpriteRenderer>().material = mats[1];
        dummy.m.ChangeBoostsAvailable(0, true);
        dummy.SetCanMove(false);

        dummy.SetTransform(new Vector2(3f, 0f), 30f);

        yield return StartCoroutine(Wait(true));

        yield return StartCoroutine(Wait());

        StartCoroutine(GiveControl());

        Rigidbody2D playerRB = player.GetComponent<Rigidbody2D>();
        while (!dummy.isDead)
        {
            player.m.turnInputValue = 0f;
            if ((player.transform.position - dummy.transform.position).magnitude < 2f && playerRB.velocity.magnitude < 6f)
            {
                player.m.UseBoost();
                player.m.SetThrust(true);
            }
            if (player.m.BoostsAvailable() == 0)
            {
                player.m.SetThrust(true);
            }

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        Transitions.instance.Fade(CallEndTutorial);
    }

    private void CallEndTutorial() { StartCoroutine(EndTutorial()); }
    private IEnumerator EndTutorial()
    {
        uiGroup.alpha = 1f;

        ResetPlayer(andBall: false);

        yield return StartCoroutine(Wait(true));

        yield return StartCoroutine(Wait());

        yield return StartCoroutine(Wait());

        yield return StartCoroutine(Wait());

        GoBackToMenu();
    }

    private void GoBackToMenu()
    {
        GameInfo.tutorial = false;
        SceneManager.LoadScene("Menu");
    }


    private IEnumerator Wait(bool waitExtra = false)
    {
        if (waitExtra) pressToContAnim.SetTrigger("HideFullSecond");
        else pressToContAnim.SetTrigger("HideHalfSecond");

        ScriptLine line = script[lineIndex];

        triangleImage.sprite = artSprites[line.image];
        text.text = line.text;

        // make sure they dont skip
        float timer = 0.5f;
        if (waitExtra) timer += 0.5f;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        while (!Input.GetKeyDown(KeyCode.W))
        {
            yield return null;
        }

        lineIndex++;
    }

    private IEnumerator GiveControl()
    {
        anim.SetTrigger("Hide");

        // make it so they have to let go of w before we start moving
        // (can also press another control or after certain amount of time to prevent frustration
        float timer = 0.75f;
        while (!Input.GetKeyUp(KeyCode.W) && !Input.GetKeyDown(KeyCode.A) && !Input.GetKeyDown(KeyCode.D) && timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        player.SetCanMove(true);
    }


    private ScriptLine[] GetScript()
    {
        // 0 = smiling, 1 = pointing, 2 = thumbs up, 3 = annoyed
        return new ScriptLine[]
        {
            new ScriptLine(0, "Hey there! Welcome to Triangle Ball."),
            new ScriptLine(1, "That triangle is you! Err... me. Err... anyways! Point is that it goes forward when you press W and turns when you press A and D."),
            new ScriptLine(2, "Why don't you give it a go? Practice moving around by following some targets!"),
            new ScriptLine(2, "Great job! Now, it wouldn't be called Triangle Ball without a ball, so let's add one of those!"),
            new ScriptLine(1, "As you may have guessed, you need to hit that ball in your opponent's goal! Go try it!"),
            new ScriptLine(3, "Wrong net..."),
            new ScriptLine(0, "Let's try scoring from a different angle! Don't worry if you're finding the movement difficult, you'll get used to it."),
            new ScriptLine(2, "Getting the hang of it yet? Not really? Oh well. Next I want to direct your attention to the yellow circles that have appeared around the map."),
            new ScriptLine(0, "These are boost pads, and by moving on top of one, it gives you the ability to boost!"),
            new ScriptLine(1, "You can use boost by pressing left shift or the spacebar. Go collect one and try it."),
            new ScriptLine(2, "Yeaaaahhhh! There are plenty of things boost is useful for. One of these uses is to demolish your opponents!"),
            new ScriptLine(1, "Make sure to hold w, and boost to demolish that poor guy."),
            new ScriptLine(3, "Well that wasn't very nice, you literally just blew him up."),
            new ScriptLine(2, "Anyways, this concludes the tutorial. There is still a ton to discover on your own though, for example, powerups!"),
            new ScriptLine(1, "Next, you should test out what you've learned by playing against a bot. I doubt you'll be able to beat one on normal difficulty, though."),
            new ScriptLine(0, "I hope you enjoy your time on Triangle Ball! Feel free to contact me on discord (owengretz#3091) if you have any ideas, suggestions, find bugs, etc.!"),
    };
    }

    // used by animator
    public void SetAlphaToZero()
    {
        uiGroup.alpha = 0f;
    }


    private void ResetPlayer(bool andBall)
    {
        player.SetCanMove(false);
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        if (andBall) ball = Instantiate(GameManager.instance.ballPrefab).transform;

        player.SetTransform(Vector2.zero, 0f);
    }
}

public class ScriptLine
{
    public string text;
    public int image;

    public ScriptLine(int _imageIndex, string _text)
    {
        text = _text;
        image = _imageIndex;
    }
}
