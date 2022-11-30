using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;


    public float ElapsedTimeSinceKickoff { get; private set; }

    public Animator vignetteAnim;
    

    public TextMeshProUGUI clockText;
    public float clockValue;
    [HideInInspector] public bool overtime;

    public Animator countdownAnim;

    private void Awake()
    {
        instance = this;
        clockValue = GameInfo.matchLength;
        if (clockValue == 0f || clockValue == -1f) { overtime = true; clockValue = 0f; }
    }

    public void ResetTimeSinceKickoff()
    {
        ElapsedTimeSinceKickoff = 0f;
    }
    public void StartCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }
    private IEnumerator CountdownRoutine()
    {
        UIManager.instance.CountdownText.text = string.Empty;

        if (GameManager.instance.menuGame)
        {
            yield return new WaitForSeconds(1f);
            ActivateGame();
            yield break;
        }

        Time.timeScale = 0f;
        GameManager.instance.SetPlayerMovement(false);
        if (overtime && GameInfo.matchLength != -1)
        {
            UIManager.instance.OvertimeCountdownText();
            countdownAnim.SetTrigger("Overtime");
            yield return null;
        }
        else
            yield return new WaitForSecondsRealtime(2f / 3f);

        countdownAnim.SetBool("Counting Down", true);
    }


    public void ActivateGame()
    {
        Time.timeScale = 1f;
        countdownAnim.SetBool("Counting Down", false);
        GameManager.instance.state = GameManager.State.Running;
        GameManager.instance.SetPlayerMovement(true);
    }

    private void Update()
    {
        if (GameManager.instance.state != GameManager.State.Running)
        {
            return;
        }

        ElapsedTimeSinceKickoff += Time.deltaTime;


        if (GameManager.instance.state == GameManager.State.Running && clockValue > 0f && !overtime)
        {
            clockValue -= Time.deltaTime;
            if (clockValue < 0f) clockValue = 0f;
        }
        else if (GameManager.instance.state == GameManager.State.Running && overtime)
        {
            clockValue += Time.deltaTime;
        }

        string minutes = Mathf.Floor(clockValue / 60).ToString("0");
        string seconds = Mathf.Floor(clockValue % 60).ToString("00");
        clockText.text = minutes + ':' + seconds;

        if (clockValue < 4f && GameManager.instance.state != GameManager.State.GameOver && GameInfo.matchLength != -1) vignetteAnim.SetBool("Show", true);


        //if (overtime || unlimitedTime)
        //{
        //    timer += Time.deltaTime;
        //}
        //else if (timer > endWith_Left)
        //{
        //    timer -= Time.deltaTime;
        //}
        //else if (!touched)
        //{
        //    waitingForTouch = true;
        //}
        //string minutes = Mathf.Floor(timer / 60).ToString("0");
        //string seconds = Mathf.Floor(timer % 60).ToString("00");
        //clockText.text = minutes + ':' + seconds;
    }


    public void Resume()
    {
        UIManager.instance.CountdownText.text = string.Empty;
        GameManager.instance.state = GameManager.State.Countdown;
        countdownAnim.SetBool("Counting Down", true);
    }
}
