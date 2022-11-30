using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager instance;

    public enum Powerups { Plunger, Spikes, Grapple, Pentagon, Clones, Bumper, Teleport, Mega, Hijack };

    public GameObject powerupCanvasPrefab;

    public Sprite[] powerupThumbnails;

    public GameObject plunger;
    public GameObject plungerRing;
    public GameObject spikes;
    public GameObject grapple;
    public GameObject bumper;
    public GameObject hijackBall;

    [HideInInspector] public List<List<Powerups>> playerPowerupQueues = new List<List<Powerups>>();
    private List<Powerups> possiblePowerupsList = new List<Powerups>();
    private System.Random rand = new System.Random();


    public void Setup()
    {
        instance = this;

        GameObject powerupCanvas = Instantiate(powerupCanvasPrefab);
        for (int i = 3; i > GameInfo.numberOfPlayers - 1; i--)
        {
            powerupCanvas.transform.GetChild(i).gameObject.SetActive(false);
        }

        // getting possible powerups
        var powerupsList = Enum.GetValues(typeof(Powerups));
        for (int i = 0; i < GameInfo.enabledPowerups.Length; i++)
        {
            if (GameInfo.enabledPowerups[i])
            {
                possiblePowerupsList.Add((Powerups)powerupsList.GetValue(i));
            }
        }


        for (int i = 0; i < GameInfo.numberOfPlayers; i++)
        {
            playerPowerupQueues.Add(new List<Powerups>());
        }

        
        StartCoroutine(WaitForPlayerSpawn());
    }

    public Powerups GetPowerup(int num)
    {
        if (playerPowerupQueues[num].Count == 0)
        {
            if (GameInfo.powerupFairness)
            {
                GeneratePowerupList(num);

                // update other players lists
                if (GameInfo.powerupEquality)
                {
                    for (int j = 0; j < GameInfo.numberOfPlayers; j++)
                    {
                        if (j != num)
                        {
                            for (int i = 0; i < playerPowerupQueues[num].Count; i++)
                            {
                                playerPowerupQueues[j].Add(playerPowerupQueues[num][i]);
                            }
                        }
                    }
                }
            }
            else
            {
                Powerups powerup = possiblePowerupsList[UnityEngine.Random.Range(0, possiblePowerupsList.Count)];

                // add that powerup to all player queues
                if (GameInfo.powerupEquality)
                {
                    for (int j = 0; j < GameInfo.numberOfPlayers; j++)
                    {
                        playerPowerupQueues[j].Add(powerup);
                    }
                }
                // only add to this players'
                else
                {
                    playerPowerupQueues[num].Add(powerup);
                }
            }
        }

        Powerups powerupDrop = playerPowerupQueues[num][0];

        playerPowerupQueues[num].RemoveAt(0);

        return powerupDrop;
    }


    private void GeneratePowerupList(int index)
    {
        // add all powerups to list
        for (int i = 0; i < possiblePowerupsList.Count; i++)
        {
            playerPowerupQueues[index].Add(possiblePowerupsList[i]);
        }

        // shuffle the list
        List<Powerups> randomList = new List<Powerups>();
        //System.Random r = new System.Random();
        
        while (playerPowerupQueues[index].Count > 0)
        {
            int randomIndex = rand.Next(0, playerPowerupQueues[index].Count);
            randomList.Add(playerPowerupQueues[index][randomIndex]); //add it to the new, random list
            playerPowerupQueues[index].RemoveAt(randomIndex); //remove to avoid duplicates
        }
        playerPowerupQueues[index] = randomList;
    }


    private IEnumerator WaitForPlayerSpawn()
    {
        while (GameManager.instance.state != GameManager.State.Running)
        {
            yield return null;
        }

        foreach (PlayerManager player in GameManager.instance.players)
        {
            PlayerPowerupScript pScript = player.gameObject.AddComponent<PlayerPowerupScript>();

            if (player.isBot) player.GetComponent<BotUtils>().powerupScript = pScript;

            //StartCoroutine(pScript.RechargePowerup());
        }

        while (GameManager.instance.state == GameManager.State.Running || GameManager.instance.state == GameManager.State.Paused 
            || GameManager.instance.state == GameManager.State.Countdown)
        {
            yield return null;
        }

        StartCoroutine(WaitForPlayerSpawn());
    }
}
