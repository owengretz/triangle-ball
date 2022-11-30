using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject Art;
    public Animator pauseMenuUIanimator;
    private bool countdownOnResume;


    public Image[] triangleFills;
    public GameObject player2Art;
    public GameObject player3Art;

    private int pausingPlayer;

    private void Start()
    {
        if (GameInfo.tutorial) Art.SetActive(false);
        if (GameInfo.numberOfPlayers < 4) player3Art.SetActive(false);
        if (GameInfo.numberOfPlayers < 3) player2Art.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TryPause(0);
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            TryPause(1);
        }
        // controller pause controls
    }


    private void TryPause(int pauser)
    {
        if (!GameManager.instance.menuGame && GameManager.instance.state != GameManager.State.GameOver)
        {
            if (GameManager.instance.state == GameManager.State.Paused)
            {
                if (pauser == pausingPlayer)
                {
                    Resume();
                }
            }
            else
            {
                Pause(pauser);
                pausingPlayer = pauser;
            }
        }
    }



    public void Resume()
    {
        pauseMenuUIanimator.SetBool("Paused", false);
        UIManager.instance.anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        if (countdownOnResume)
        {
            countdownOnResume = false;
            TimeManager.instance.Resume();
        }
        else
        {
            GameManager.instance.state = GameManager.State.Running;
            Time.timeScale = 1f;
            GameManager.instance.SetPlayerMovement(false);
        }
    }

    private void Pause(int pauser)
    {
        SetArt(pauser);

        if (GameManager.instance.state == GameManager.State.Running || GameManager.instance.state == GameManager.State.Countdown)
        {
            countdownOnResume = true;
        }
        if (GameInfo.tutorial) countdownOnResume = false;

        UIManager.instance.anim.updateMode = AnimatorUpdateMode.Normal;
        pauseMenuUIanimator.SetBool("Paused", true);
        GameManager.instance.state = GameManager.State.Paused;
        Time.timeScale = 0f;

        GameManager.instance.SetPlayerMovement(false);
    }

    public void LoadMenu()
    {
        StartCoroutine(LoadMenuRoutine());
    }
    private IEnumerator LoadMenuRoutine()
    {
        GameManager.instance.SpawnCamera();
        Time.timeScale = 1f;
        GameManager.instance.state = GameManager.State.Running;

        ReplayHandle handle = GameManager.instance.instantReplay;
        if (ReplayManager.IsReplaying(handle))
        {
            ReplayManager.StopPlaybackDelayed(handle);
            while (ReplayManager.IsReplaying(handle))
            {
                yield return null;
            }
        }
        if (ReplayManager.IsRecording(handle)) ReplayManager.StopRecording(ref handle);

        GameInfo.tutorial = false;
        SceneManager.LoadScene("Menu");
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void SetArt(int pauser)
    {
        Material[] mats = GameManager.instance.playerMaterials;
        for (int i = 0; i < 4; i++)
        {
            if (i == pauser)
            {
                triangleFills[0].material = mats[i];
            }
            else if (i < pauser)
            {
                triangleFills[i + 1].material = mats[i];
            }
            else // i > pauser
            {
                triangleFills[i].material = mats[i];
            }
        }
    }
}
