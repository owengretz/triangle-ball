using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UltimateReplay.Storage;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour
{
    public GameObject threePlayerAddition;

    public Image[] playerColours;
    public Image[] buttonImages;


    public void Display()
    {
        GameManager g = GameManager.instance;
        int winner = g.winnerIndex;

        foreach (Image image in buttonImages) image.material = g.playerMaterials[winner];

        playerColours[0].material = g.playerMaterials[winner];

        switch (winner)
        {
            case 0:
                playerColours[1].material = g.playerMaterials[1];
                playerColours[2].material = g.playerMaterials[2];
                break;
            case 1:
                playerColours[1].material = g.playerMaterials[0];
                playerColours[2].material = g.playerMaterials[2];
                break;
            case 2:
                playerColours[1].material = g.playerMaterials[0];
                playerColours[2].material = g.playerMaterials[1];
                break;
        }

        if (/*g.mode != GameManager.Mode.OneVsOneVsOne*/GameInfo.numberOfPlayers != 3)
        {
            threePlayerAddition.SetActive(false);
            playerColours[2].gameObject.SetActive(false);
        }
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Menu()
    {
        SceneManager.LoadScene("Menu");

        //if (ReplayManager.IsReplaying(GameManager.instance.fullReplay))
        //{
        //    ReplayManager.StopPlayback(ref GameManager.instance.fullReplay);
        //}
        //GameManager.instance.fullReplayStorage.Dispose();
    }

    public void SaveReplay()
    {
        
    }
}
