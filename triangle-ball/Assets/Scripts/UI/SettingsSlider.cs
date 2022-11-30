using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsSlider : MonoBehaviour
{
    public enum Property 
    { 
        PowerupCooldown, 
        RespawnCooldown, 
        BoostRespawnTime, 
        KickoffBoostAmount, 
        PlungerDuration, 
        PlungerRange, 
        PlungerForce, 
        PlungerLatchSpeed,
        SpikesDuration,
        SpikesReleaseSpeed,
        GrappleLatchSpeed,
        GrappleGrappleSpeed,
        PentagonDuration,
        ClonesDuration,
        ClonesAmount,
        BumperDuration,
        BumperLaunchSpeed,
        BumperBumpForce,
        TeleportDelay,
        MegaDuration,
        MegaMassIncrease,
        HijackDuration,
        HijackMoveForce,
        PlayerThrustForce,
        PlayerBoostForce,
        PlayerTurnSpeed,
        PlayerMass,
        PlayerDrag,
        PlayerDemolishThreshold,
        PlayerBumpForce,
        BallMass,
        BallDrag,
        BallBounciness
    };

    public static Dictionary<Property, float> defaults = new Dictionary<Property, float>()
    {
        { Property.PowerupCooldown, 8f },
        { Property.RespawnCooldown, 2f },
        { Property.BoostRespawnTime, 6f },
        { Property.KickoffBoostAmount, 1f },
        { Property.PlungerDuration, 1.5f },
        { Property.PlungerRange, 10f },
        { Property.PlungerForce, 10f },
        { Property.PlungerLatchSpeed, 30f },
        { Property.SpikesDuration, 8f },
        { Property.SpikesReleaseSpeed, 5f },
        { Property.GrappleLatchSpeed, 40f },
        { Property.GrappleGrappleSpeed, 10f },
        { Property.PentagonDuration, 8f },
        { Property.ClonesDuration, 8f },
        { Property.ClonesAmount, 2f },
        { Property.BumperDuration, 8f },
        { Property.BumperLaunchSpeed, 15f },
        { Property.BumperBumpForce, 20f },
        { Property.TeleportDelay, 0.5f },
        { Property.MegaDuration, 8f },
        { Property.MegaMassIncrease, 2f },
        { Property.HijackDuration, 8f },
        { Property.HijackMoveForce, 3.2f },
        { Property.PlayerThrustForce, 650f },
        { Property.PlayerBoostForce, 12f },
        { Property.PlayerTurnSpeed, 270f },
        { Property.PlayerMass, 2f },
        { Property.PlayerDrag, 1f },
        { Property.PlayerDemolishThreshold, 6.5f },
        { Property.PlayerBumpForce, 30f },
        { Property.BallMass, 1f },
        { Property.BallDrag, 0.5f },
        { Property.BallBounciness, 1f }
    };


    [SerializeField] private float[] possibleSliderValues;
    [SerializeField] private string suffix = string.Empty;
    [HideInInspector] public Property property;

    private Slider slider;
    private TMP_Text label;
    private ExtraSettings extraSettings;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        label = GetComponentInChildren<TMP_Text>();

        slider.minValue = 0;
        slider.maxValue = possibleSliderValues.Length - 1;
        slider.wholeNumbers = true;

        //slider.value = System.Array.IndexOf(possibleSliderValues, defaults[property]);

        extraSettings = FindObjectOfType<ExtraSettings>();
    }

    public void SliderValueChanged(float newIndex)
    {
        int valueIndex = (int)newIndex;
        string stringValue = possibleSliderValues[valueIndex].ToString();
        if (stringValue == "-1") stringValue = "∞";
        label.text = stringValue + suffix;

        SetGameInfoProperty(possibleSliderValues[valueIndex]);
        extraSettings.SettingChanged();
    }

    public void ChangeValue(float value)
    {
        slider.value = System.Array.IndexOf(possibleSliderValues, value);
        SetGameInfoProperty(value); // if the value of the slider stays the same then it wont actually change the property
        //extraSettings.SettingChanged();
    }

    public void SetGameInfoProperty(float value)
    {
        switch (property)
        {
            case Property.PowerupCooldown: GameInfo.powerupCooldown = value; break;
            case Property.RespawnCooldown: GameInfo.respawnCooldown = value; break;
            case Property.BoostRespawnTime: GameInfo.boostRespawnTime = value; break;
            case Property.KickoffBoostAmount: GameInfo.kickoffBoostAmount = value; break;
            case Property.PlungerDuration: GameInfo.plungerDuration = value; break;
            case Property.PlungerRange: GameInfo.plungerRange = value; break;
            case Property.PlungerForce: GameInfo.plungerForce = value; break;
            case Property.PlungerLatchSpeed: GameInfo.plungerLatchSpeed = value; break;
            case Property.SpikesDuration: GameInfo.spikesDuration = value; break;
            case Property.SpikesReleaseSpeed: GameInfo.spikesReleaseSpeed = value; break;
            case Property.GrappleLatchSpeed: GameInfo.grappleLatchSpeed = value; break;
            case Property.GrappleGrappleSpeed: GameInfo.grappleGrappleSpeed = value; break;
            case Property.PentagonDuration: GameInfo.pentagonDuration = value; break;
            case Property.ClonesDuration: GameInfo.clonesDuration = value; break;
            case Property.ClonesAmount: GameInfo.clonesAmount = value; break;
            case Property.BumperDuration: GameInfo.bumperDuration = value; break;
            case Property.BumperLaunchSpeed: GameInfo.bumperLaunchSpeed = value; break;
            case Property.BumperBumpForce: GameInfo.bumperBumpForce = value; break;
            case Property.TeleportDelay: GameInfo.teleportDelay = value; break;
            case Property.MegaDuration: GameInfo.megaDuration = value; break;
            case Property.MegaMassIncrease: GameInfo.megaMassIncrease = value; break;
            case Property.HijackDuration: GameInfo.hijackDuration = value; break;
            case Property.HijackMoveForce: GameInfo.hijackMoveForce = value; break;
            case Property.PlayerThrustForce: GameInfo.playerThrustForce = value; break;
            case Property.PlayerBoostForce: GameInfo.playerBoostForce = value; break;
            case Property.PlayerTurnSpeed: GameInfo.playerTurnSpeed = value; break;
            case Property.PlayerMass: GameInfo.playerMass = value; break;
            case Property.PlayerDrag: GameInfo.playerDrag = value; break;
            case Property.PlayerDemolishThreshold: GameInfo.playerDemolishThreshold = value; break;
            case Property.PlayerBumpForce: GameInfo.playerBumpForce = value; break;
            case Property.BallMass: GameInfo.ballMass = value; break;
            case Property.BallDrag: GameInfo.ballDrag = value; break;
            case Property.BallBounciness: GameInfo.ballBounciness = value; break;

            default:
                Debug.LogError("You need to add the " + property + "property to the switch block in SettingsSlider.");
                break;

        }

        PlayerPrefs.SetFloat(property.ToString(), value);
    }

    
    public void LoadPlayerPref()
    {
        ChangeValue(PlayerPrefs.GetFloat(property.ToString()));
    }

}
