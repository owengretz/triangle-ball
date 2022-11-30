using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public SpriteRenderer blurPanelRend;

    private Animator anim;

    private void Start()
    {
        anim = GetComponentInParent<Animator>();
        StartCoroutine(StartBackgroundGame());
    }


    private IEnumerator StartBackgroundGame()
    {
        GameInfo.instance.ResetDefaults();
        //GameInfo.mapInfo = FindObjectOfType<GameInfo>().defaultMap;
        //GameInfo.powerupsOn = false;
        //GameInfo.numberOfPlayers = 2;
        //GameInfo.botPlayers = new List<int>() { 2, 2, 2, 2 };
        //GameInfo.matchLength = -1f;
        //GameInfo.maxScore = 9999;
        //GameInfo.showBotCursors = false;


        var op = SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => op.isDone);

        TimeManager.instance.vignetteAnim.gameObject.SetActive(false);
        GameManager.instance.menuGame = true;
        UIManager.instance.ScoreboardBackground.gameObject.SetActive(false);
    }


    public void Play()
    {
        StopRecording();
        SceneManager.LoadScene("CreateGame");
    }

    public void Tutorial()
    {
        StopRecording();
        GameInfo.tutorial = true;
        SceneManager.LoadScene("GameScene");
        //LoadGameScene
        //Transitions.instance.Fade(LoadGameScene);
    }
    //private void LoadGameScene()
    //{
    //    SceneManager.LoadScene("GameScene");
    //}


    private void StopRecording()
    {
        if (UltimateReplay.ReplayManager.IsRecording(GameManager.instance.instantReplay)) UltimateReplay.ReplayManager.StopRecording(ref GameManager.instance.instantReplay);
    }


    public void Quit()
    {
        Application.Quit();
    }

    public void ToggleTitle()
    {
        bool val = !anim.GetBool("Show Title");
        //bool val = false;
        anim.SetBool("Show Title", val);
        blurPanelRend.enabled = val;
    }
}
