using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject scoreboard;

    [HideInInspector] public Animator anim;

    public TextMeshProUGUI[] scoreTexts;


    public Image ScoreboardBackground;
    public Sprite ThreeTeamScoreboardBackgroundImage;

    public RectTransform[] ScoreboardElements;
    public RectTransform[] ThreeTeamScoreboardPositions;

    public TextMeshProUGUI CountdownText;

    public RectTransform playerIndicatorHolder;
    public Image[] playerIndicators;

    public GameObject endGameUI;

    public TMP_Text fpsDisplay;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (/*GameManager.instance.mode == GameManager.Mode.OneVsOneVsOne*/GameInfo.numberOfPlayers == 3)
        {
            ScoreboardBackground.sprite = ThreeTeamScoreboardBackgroundImage;
            ScoreboardElements[3].gameObject.SetActive(true);
            for (int i = 0; i < ScoreboardElements.Length; i++)
            {
                ScoreboardElements[i].anchoredPosition = ThreeTeamScoreboardPositions[i].anchoredPosition;
            }
        }

        anim = GetComponent<Animator>();
    }

    public void OvertimeCountdownText()
    {
        CountdownText.text = "OVERTIME";
        
    }
    public void ChangeCountdownText()
    {
        if (CountdownText.text == string.Empty || CountdownText.text == "OVERTIME")
        {
            CountdownText.text = "3";
        }
        else if (int.Parse(CountdownText.text) == 1)
        {
            CountdownText.text = "GO!";
            ActivateGame();
        }
        else
        {
            CountdownText.text = (int.Parse(CountdownText.text) - 1).ToString();
        }
    }
    public void ActivateGame()
    {
        FindObjectOfType<TimeManager>().ActivateGame();
        //Invoke("ResetCountdownText", 1f);
    }
    //private void ResetCountdownText()
    //{
    //    CountdownText.text = string.Empty;
    //}

    public void UpdateScoreText()
    {
        for (int i = 0; i < GameManager.instance.scoreValues.Count; i++)
        {
            scoreTexts[i].text = GameManager.instance.scoreValues[i].ToString();
        }
    }

    public void DeactivatePlayerIndicator(int index)
    {
        playerIndicators[index].gameObject.SetActive(false);
    }

    public void EndGame()
    {
        anim.SetTrigger("End Game");
        endGameUI.GetComponent<EndGameUI>().Display();
    }

    private void Update()
    {
        float fps = Mathf.Round(1 / Time.unscaledDeltaTime);
        fpsDisplay.text = fps.ToString();
    }
}
