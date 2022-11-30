using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class CreateGame : MonoBehaviour
{
    public Animator lockedMessageAnim;

    [Header("Map")]
    public Image mapImage;
    public MapInfo[] mapData;
    private int mapIndex;
    public GameObject mapLockedIcon;
    private int numMaps;
    private readonly int lockedMaps = 1;

    [Header("Ball Skin")]
    public Image ballSkinImage;
    public Sprite[] ballSkins;
    private int ballSkinIndex;
    public GameObject animatedIcon;
    private TMP_Text animatedIconText;
    private readonly string[] animatedIconTooltipText = { "Looks at the nearest player", "Changes colour on contact", "Displays the time" };
    //private AnimatedBallIcon tooltipScript;
    public GameObject ballSkinLockedIcon;
    private int numBallSkins;
    private readonly int animatedBallSkins = 3;
    private readonly int lockedBallSkins = 5;
    

    [Header("Bots")]
    public RectTransform playerIconsHolder;
    public Image[] playerIconImages;
    public Sprite[] botIcons; // human, easy, normal
    public TextMeshProUGUI botText;
    private int[] botIconIndices = new int[] { 0, 0, 0, 0 };
    private int numberOfBots = 0;
    //private readonly string[] botText = new string[]

    [Header("Gamemode")]
    public TextMeshProUGUI powerupText;
    private readonly string[] powerupNames = new string[] { "On", "Off" };
    private int powerupIndex;

    [Header("Number Of Players")]
    public TextMeshProUGUI numPlayersText;
    public Button[] numPlayersArrows;
    private int numPlayersIndex;
    private int numPlayers;

    [Header("Match Length")]
    public TMP_Text matchLengthText;
    private readonly string[] matchLengthNames = new string[] { "SD", "0:30", "1:00", "1:30", "2:00", "2:30", "3:00", "3:30", "4:00", "4:30", "5:00", "∞" };
    private int matchLengthIndex;

    [Header("Max Score")]
    public TMP_Text maxScoreText;
    private readonly string[] maxScoreNames = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "∞" };
    private int maxScoreIndex;



    private void Start()
    {
        mapIndex = 0;
        numMaps = mapData.Length;
        ballSkinIndex = 0;
        numBallSkins = ballSkins.Length;
        animatedIconText = animatedIcon.GetComponentInChildren<TMP_Text>();
        //tooltipScript = transform.GetComponentInChildren<AnimatedBallIcon>();
        powerupIndex = 0;
        numPlayersIndex = 0;
        //numPlayers = int.Parse(numPlayersNames[numPlayersIndex]);
        //ChangeNumPlayers(0);
        matchLengthIndex = 6;
        maxScoreIndex = 9;

        LoadPlayerPrefs();
        ChangeMap(0);
        ChangeBallSkin(0);
    }

    private void LoadPlayerPrefs()
    {
        //PlayerPrefs.DeleteAll();

        GetComponentInChildren<ExtraSettings>().LoadPlayerPrefs();

        if (!PlayerPrefs.HasKey("FirstTime"))
        {
            PlayerPrefs.SetString("FirstTime", "False");

            PlayerPrefs.SetString("Match Length", "3:00");
            PlayerPrefs.SetString("Max Score", "∞");
            PlayerPrefs.SetString("Powerups On", "On");
            PlayerPrefs.SetInt("Map Index", 0);
            PlayerPrefs.SetInt("Ball Skin", 0);
            PlayerPrefs.SetInt("Number of Players", 2);
            ChangeNumPlayers(0);
            PlayerPrefs.SetString("Bot Players", "0000");
        }
        else
        {
            string matchLength = PlayerPrefs.GetString("Match Length");
            string maxScore = PlayerPrefs.GetString("Max Score");
            int mIndex = PlayerPrefs.GetInt("Map Index");
            int bsIndex = PlayerPrefs.GetInt("Ball Skin");
            int nPlayers = PlayerPrefs.GetInt("Number of Players");


            while (matchLengthText.text != matchLength) ChangeMatchLength(1);
            while (maxScoreText.text != maxScore) ChangeMaxScore(1);
            if (PlayerPrefs.GetString("Powerups On") == "Off") ChangePowerups(1);
            while (mapIndex != mIndex) ChangeMap(1);
            while (ballSkinIndex != bsIndex) ChangeBallSkin(1);
            while (numPlayers != nPlayers) ChangeNumPlayers(1);
            for (int i = 0; i < 4; i++)
            {
                char playerPref = PlayerPrefs.GetString("Bot Players")[i];
                if (playerPref == '1')
                {
                    ChangeBots(i);
                }
                else if (playerPref == '2')
                {
                    ChangeBots(i);
                    ChangeBots(i);
                }
            }
        }
    }



    public void ChangeMap(int direction)
    {
        mapIndex += direction;
        if (mapIndex > mapData.Length - 1) mapIndex = 0;
        if (mapIndex < 0) mapIndex = mapData.Length - 1;
        
        MapInfo map = mapData[mapIndex];
        mapImage.sprite = map.thumbnail;
        EnableOrDisableBotToggles(map.botsAllowed);

        numPlayersIndex = 0;
        //numPlayersText.text = map.playersSupported[0];
        if (!map.playersSupported.Contains(numPlayers.ToString()))
        {
            ChangeNumPlayers(-numPlayersIndex);
        }

        

        mapLockedIcon.SetActive(mapIndex >= numMaps - lockedMaps);
        CheckLockedMessage();

        PlayerPrefs.SetInt("Map Index", mapIndex);
    }

    public void ChangeBallSkin(int direction)
    {
        ballSkinIndex += direction;

        if (ballSkinIndex > ballSkins.Length - 1) ballSkinIndex = 0;
        if (ballSkinIndex < 0) ballSkinIndex = ballSkins.Length - 1;

        ballSkinImage.sprite = ballSkins[ballSkinIndex];



        animatedIcon.SetActive(ballSkinIndex >= numBallSkins - animatedBallSkins);
        if (ballSkinIndex >= numBallSkins - animatedBallSkins)
            animatedIconText.text = animatedIconTooltipText[animatedBallSkins - numBallSkins + ballSkinIndex];
        ballSkinLockedIcon.SetActive(ballSkinIndex >= numBallSkins - lockedBallSkins);
        CheckLockedMessage();


        PlayerPrefs.SetInt("Ball Skin", ballSkinIndex);
    }

    public void CheckLockedMessage()
    {
        lockedMessageAnim.SetBool("Show", ballSkinIndex >= numBallSkins - lockedBallSkins || mapIndex >= numMaps - lockedMaps);
    }
    public void ShowPurchaseScreen()
    {

    }

    private void EnableOrDisableBotToggles(bool enable)
    {
        foreach (Image icon in playerIconImages) icon.GetComponent<Button>().interactable = enable;

        UpdateBotText();
    }
    public void ChangeBots(int playerNumber)
    {
        botIconIndices[playerNumber]++;
        if (botIconIndices[playerNumber] > 2) botIconIndices[playerNumber] = 0;
        playerIconImages[playerNumber].sprite = botIcons[botIconIndices[playerNumber]];
        UpdateBotText();

        string bots = PlayerPrefs.GetString("Bot Players");
        PlayerPrefs.SetString("Bot Players", bots.Substring(0, playerNumber) + botIconIndices[playerNumber].ToString() + bots.Substring(playerNumber + 1));
    }
    private void UpdateBotText()
    {
        numberOfBots = 0;

        if (!mapData[mapIndex].botsAllowed)
        {
            foreach (Image icon in playerIconImages) icon.sprite = botIcons[0];
            botText.text = "Bots Not Supported";
        }
        else
        {
            foreach (Image icon in playerIconImages) if (icon.sprite != botIcons[0] && icon.IsActive()) numberOfBots++;

            string message = "";
            if (numberOfBots >= 1)
            {
                int easy = 0;
                int normal = 0;
                for (int i = 0; i < numPlayers; i++)
                {
                    if (playerIconImages[i].sprite == botIcons[1]) easy++; 
                    else if (playerIconImages[i].sprite == botIcons[2]) normal++;
                }
                string easyPlural = easy >= 2 ? "s" : "";
                string normalPlural = normal >= 2 ? "s" : "";
                if (easy >= 1) message += easy.ToString() + " Easy Bot" + easyPlural;
                if (easy >= 1 && normal >= 1) message += ", ";
                if (normal >= 1) message += normal.ToString() + " Normal Bot" + normalPlural;
            }
            else
            {
                message = "Click To Add Bots";
            }

            botText.text = message;
        }
    }

    public void ChangeNumPlayers(int direction)
    {
        numPlayersIndex = ChangeInfo(mapData[mapIndex].playersSupported, numPlayersText, numPlayersIndex, direction);

        foreach (Button arrow in numPlayersArrows) arrow.interactable = mapData[mapIndex].playersSupported.Length != 1;

        numPlayers = int.Parse(mapData[mapIndex].playersSupported[numPlayersIndex]);
        for (int i = 0; i < playerIconImages.Length; i++)
        {
            playerIconImages[i].enabled = i < numPlayers ? true : false;
        }
        playerIconsHolder.anchoredPosition = new Vector2(50f * (4 - numPlayers), playerIconsHolder.anchoredPosition.y);

        UpdateBotText();

        PlayerPrefs.SetInt("Number of Players", numPlayers);
    }

    public void ChangePowerups(int direction)
    {
        powerupIndex = ChangeInfo(powerupNames, powerupText, powerupIndex, direction);

        PlayerPrefs.SetString("Powerups On", powerupText.text);
    }

    public void ChangeMatchLength(int direction)
    {
        matchLengthIndex = ChangeInfo(matchLengthNames, matchLengthText, matchLengthIndex, direction);

        PlayerPrefs.SetString("Match Length", matchLengthText.text);
    }

    public void ChangeMaxScore(int direction)
    {
        maxScoreIndex = ChangeInfo(maxScoreNames, maxScoreText, maxScoreIndex, direction);

        PlayerPrefs.SetString("Max Score", maxScoreText.text);
    }

    private int ChangeInfo(string[] nameArray, TMP_Text text, int currentIndex, int direction)
    {
        // if we go left at first item loop around to the last item
        if (currentIndex == 0 && direction == -1)
        {
            text.text = nameArray[nameArray.Length - 1];
        }
        // if we go right at last item loop around to the first
        else if (currentIndex == nameArray.Length - 1 && direction == 1)
        {
            text.text = nameArray[0];
        }
        else
        {
            text.text = nameArray[currentIndex + direction];
        }
        currentIndex = System.Array.IndexOf(nameArray, text.text);

        return currentIndex;
    }

    public void Confirm()
    {
        bool powerupsOn = powerupText.text == "On";
        //Enum.TryParse(powerupText.text, out GameInfo.Gamemode gamemode);

        List<int> botPlayers = new List<int>();
        for (int i = 0; i < playerIconImages.Length; i++) botPlayers.Add(botIconIndices[i]);

        float matchLength;
        if (matchLengthText.text == "∞") matchLength = -1f;
        else if (matchLengthText.text == "SD") matchLength = 0f;
        else
        {
            string[] matchLengthStringList = matchLengthText.text.Split(':');
            matchLength = float.Parse(matchLengthStringList[0]) * 60 + float.Parse(matchLengthStringList[1]);
        }

        int maxScore = maxScoreText.text == "∞" ? 99999 : int.Parse(maxScoreText.text);

        int specialBallIndex = -1;
        if (ballSkinIndex >= numBallSkins - animatedBallSkins)
            specialBallIndex = animatedBallSkins - numBallSkins + ballSkinIndex;

        GameInfo.SetInfo(mapData[mapIndex], ballSkins[ballSkinIndex], specialBallIndex, powerupsOn, numPlayers, botPlayers, matchLength, maxScore);

        SceneManager.LoadScene("GameScene");
    }


    public void Back()
    {
        SceneManager.LoadScene("Menu");
    }
}
