using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPowerupScript : MonoBehaviour
{
    [HideInInspector] public PowerupManager.Powerups powerupDrop; // used by BotScript

    private PlayerManager manager;
    private PlayerMovement movement;
    private string powerupButtonName;
    [HideInInspector] public bool isPowerupButtonPressed; // used by BotScript

    [HideInInspector] public bool canUsePowerup; // used by powerup scripts
    [HideInInspector] public bool usingPowerup; // used by powerup scripts

    private Slider powerupFillSlider;
    private Image powerupThumbnail;

    private void Start()
    {
        manager = GetComponent<PlayerManager>();
        movement = GetComponent<PlayerMovement>();
        powerupButtonName = movement.powerupButtonName;

        GameObject playerIcon = GameObject.Find("Player " + manager.playerNumber + " Powerup Icon");

        powerupFillSlider = playerIcon.GetComponentInChildren<Slider>();

        powerupThumbnail = playerIcon.transform.GetChild(2).GetComponent<Image>();

        StartCoroutine(RechargePowerup());
    }

    private void Update()
    {
        if (movement.isBot)
            return;

        isPowerupButtonPressed = Sinput.GetButtonDown(powerupButtonName);
    }

    public IEnumerator RechargePowerup()
    {
        //var powerupsList = Enum.GetValues(typeof(PowerupManager.Powerups));
        //powerupDrop = (PowerupManager.Powerups)powerupsList.GetValue(UnityEngine.Random.Range(0, powerupsList.Length));
        //powerupDrop = possiblePoweurps[UnityEngine.Random.Range(0, possiblePoweurps.Count)];
        powerupDrop = PowerupManager.instance.GetPowerup(manager.playerNumber);

        //Debug.Log(powerupDrop);
        //if (GetComponent<PlayerManager>().playerNumber == 0) Debug.Log(powerupDrop);
        //powerupDrop = PowerupManager.Powerups.Hijack;
        //if (GetComponent<PlayerManager>().playerNumber == 0) powerupDrop = PowerupManager.Powerups.Hijack;
        //if (GetComponent<PlayerManager>().playerNumber == 1) powerupDrop = PowerupManager.Powerups.Hijack;


        powerupFillSlider.maxValue = GameInfo.powerupCooldown == 0 ? 1 : GameInfo.powerupCooldown;

        float timer = 0f;
        while (timer < GameInfo.powerupCooldown)
        {
            timer += Time.deltaTime; // paused powerup glitch
            powerupFillSlider.value = timer;

            yield return null;
        }

        // so that the game doesnt break using infinite powerups
        if (GameInfo.powerupCooldown == 0f)
            yield return null;

        powerupFillSlider.value = powerupFillSlider.maxValue;

        //GameObject.Find("Player " + manager.playerNumber + " Powerup").GetComponent<TMPro.TMP_Text>().text = powerupDrop.ToString();

        var powerupList = Enum.GetValues(typeof(PowerupManager.Powerups));

        powerupThumbnail.enabled = true;
        powerupThumbnail.sprite = PowerupManager.instance.powerupThumbnails[Array.IndexOf(powerupList, powerupDrop)];
        

        float powerupDuration = 8f;


        canUsePowerup = true;

        GameObject plungerRing = null;
        if (powerupDrop == PowerupManager.Powerups.Plunger)
        {
            plungerRing = Instantiate(PowerupManager.instance.plungerRing, transform);
            plungerRing.transform.localScale = new Vector3(GameInfo.plungerRange, GameInfo.plungerRange, 1);
        }

        while (!isPowerupButtonPressed || !movement.canMove/* && !movement.isBot*/)
        {
            if (powerupDrop == PowerupManager.Powerups.Plunger && plungerRing != null) plungerRing.GetComponent<SpriteRenderer>().enabled = !manager.isDead;

            yield return null;
        }

        canUsePowerup = false;
        usingPowerup = true;


        BallManager ball = FindObjectOfType<BallManager>(); // might cause problems?

        Powerup powerup;

        switch (powerupDrop)
        {
            case PowerupManager.Powerups.Plunger:
                powerup = gameObject.AddComponent<PlungerPowerup>();
                powerupDuration = GameInfo.plungerDuration;
                break;
            case PowerupManager.Powerups.Mega:
                powerup = gameObject.AddComponent<MegaPowerup>();
                powerupDuration = GameInfo.megaDuration;
                break;
            case PowerupManager.Powerups.Spikes:
                powerup = gameObject.AddComponent<SpikesPowerup>();
                powerupDuration = GameInfo.spikesDuration;
                break;
            case PowerupManager.Powerups.Clones:
                powerup = gameObject.AddComponent<ClonesPowerup>();
                powerupDuration = GameInfo.clonesDuration;
                break;
            case PowerupManager.Powerups.Grapple:
                powerup = gameObject.AddComponent<GrapplePowerup>();
                if (ball != null) powerupDuration = (ball.transform.position - transform.position).magnitude;
                else powerupDuration = 1f;
                break;
            case PowerupManager.Powerups.Bumper:
                powerup = gameObject.AddComponent<BumperPowerup>();
                powerupDuration = GameInfo.bumperDuration;
                break;
            case PowerupManager.Powerups.Teleport:
                powerup = gameObject.AddComponent<TeleportPowerup>();
                powerupDuration = GameInfo.teleportDelay;
                break;
            case PowerupManager.Powerups.Hijack:
                powerup = gameObject.AddComponent<HijackBallPowerup>();
                powerupDuration = GameInfo.hijackDuration;
                break;
            case PowerupManager.Powerups.Pentagon:
                powerup = gameObject.AddComponent<PentagonPowerup>();
                powerupDuration = GameInfo.pentagonDuration;
                break;
            default:
                Debug.LogError("Powerup " + powerupDrop + " not found fix ur game");
                powerup = null;
                break;
        }


        powerupFillSlider.maxValue = powerupDuration;
        powerupFillSlider.value = powerupDuration;

        powerup.UsePowerup();


        while (!powerup.endPowerup)
        {
            yield return null;

            powerupFillSlider.value -= Time.deltaTime;

            if (powerupDrop == PowerupManager.Powerups.Grapple && ball != null)
            {
                powerupFillSlider.value = (ball.transform.position - transform.position).magnitude;
            }
        }

        if (powerupDrop == PowerupManager.Powerups.Plunger && plungerRing != null) Destroy(plungerRing);

        powerup.EndPowerup();
        Destroy(powerup);
        usingPowerup = false;

        //GameObject.Find("Player " + manager.playerNumber + " Powerup").GetComponent<TMPro.TMP_Text>().text = "-";

        powerupThumbnail.enabled = false;
        powerupFillSlider.value = 0f;


        StartCoroutine(RechargePowerup());
    }



    private void OnDisable()
    {
        if (powerupThumbnail == null) return;

        powerupThumbnail.enabled = false;
        powerupFillSlider.value = 0f;
    }
}
