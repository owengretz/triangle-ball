using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExtraSettings : MonoBehaviour
{
    #region Variables

    [SerializeField] private Toggle[] powerupToggles;
    private bool changingQuickMode;
    private bool changingAllToggles;
    //private bool[] enabledPowerups = new bool[Enum.GetNames(typeof(PowerupManager.Powerups)).Length];

    [SerializeField] private SettingsSlider[] sliders;
    [SerializeField] private SettingsToggle[] toggles;

    [SerializeField] private GameObject[] experimentalSettings;

    [SerializeField] private RectTransform content;
    [SerializeField] private float noExperimentalContentHeight;
    [SerializeField] private float withExperimentalContentHeight;


    [SerializeField] private CustomGamemode[] customGamemodes;
    [SerializeField] private TMP_Text quickModeText;
    private int quickModeIndex;

    [SerializeField] private GameObject friendlyDemosToggle;

    #endregion


    private void Start()
    {
        for (int i = 0; i < GameInfo.enabledPowerups.Length; i++) GameInfo.enabledPowerups[i] = powerupToggles[i].isOn;
        ToggleExperimentalSettings(false);

        quickModeIndex = 0;

        //LoadPlayerPrefs();
        //PlayerPrefs.DeleteAll();
    }

    public void LoadPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("FirstTime"))
        {
            //PlayerPrefs.SetString("FirstTime", "False");

            foreach (SettingsSlider slider in sliders) slider.ChangeValue(SettingsSlider.defaults[slider.property]);
            foreach (SettingsToggle toggle in toggles) toggle.ChangeValue(SettingsToggle.defaults[toggle.property]);
            for (int i = 0; i < powerupToggles.Length; i++) SetPowerupToggleValue(i, true);
            PlayerPrefs.SetString("Gamemode Name", "Default");
            PlayerPrefs.SetInt("Quickmode Index", 0);
        }
        else
        {
            string gameModeName = PlayerPrefs.GetString("Gamemode Name");
            foreach (SettingsSlider slider in sliders) slider.LoadPlayerPref();
            foreach (SettingsToggle toggle in toggles) toggle.LoadPlayerPref();
            for (int i = 0; i < powerupToggles.Length; i++) SetPowerupToggleValue(i, PlayerPrefs.GetInt("Powerup" + i) == 1);
            quickModeText.text = gameModeName;
            PlayerPrefs.SetString("Gamemode Name", gameModeName);
            quickModeIndex = PlayerPrefs.GetInt("Quickmode Index");
        }
    }

    public void QuickMode(int direction)
    {
        changingQuickMode = true;

        if (quickModeText.text == "Custom")
        {
            quickModeIndex = 0;
        }
        else
        {
            quickModeIndex += direction;
            if (quickModeIndex > customGamemodes.Length - 1) quickModeIndex = 0;
            if (quickModeIndex < 0) quickModeIndex = customGamemodes.Length - 1;
        }

        CustomGamemode mode = customGamemodes[quickModeIndex];

        SetPowerupToggleValue(0, mode.plungerEnabled);
        SetPowerupToggleValue(1, mode.spikesEnabled);
        SetPowerupToggleValue(2, mode.grappleEnabled);
        SetPowerupToggleValue(3, mode.pentagonEnabled);
        SetPowerupToggleValue(4, mode.clonesEnabled);
        SetPowerupToggleValue(5, mode.bumperEnabled);
        SetPowerupToggleValue(6, mode.teleportEnabled);
        SetPowerupToggleValue(7, mode.megaEnabled);
        SetPowerupToggleValue(8, mode.hijackEnabled);

        sliders[0].ChangeValue(GetValue(mode, SettingsSlider.Property.PowerupCooldown));
        sliders[1].ChangeValue(GetValue(mode, SettingsSlider.Property.RespawnCooldown));
        sliders[2].ChangeValue(GetValue(mode, SettingsSlider.Property.BoostRespawnTime));
        sliders[3].ChangeValue(GetValue(mode, SettingsSlider.Property.KickoffBoostAmount));
        sliders[4].ChangeValue(GetValue(mode, SettingsSlider.Property.PlungerDuration));
        sliders[5].ChangeValue(GetValue(mode, SettingsSlider.Property.PlungerRange));
        sliders[6].ChangeValue(GetValue(mode, SettingsSlider.Property.PlungerForce));
        sliders[7].ChangeValue(GetValue(mode, SettingsSlider.Property.PlungerLatchSpeed));
        sliders[8].ChangeValue(GetValue(mode, SettingsSlider.Property.SpikesDuration));
        sliders[9].ChangeValue(GetValue(mode, SettingsSlider.Property.SpikesReleaseSpeed));
        sliders[10].ChangeValue(GetValue(mode, SettingsSlider.Property.GrappleLatchSpeed));
        sliders[11].ChangeValue(GetValue(mode, SettingsSlider.Property.GrappleGrappleSpeed));
        sliders[12].ChangeValue(GetValue(mode, SettingsSlider.Property.PentagonDuration));
        sliders[13].ChangeValue(GetValue(mode, SettingsSlider.Property.ClonesDuration));
        sliders[14].ChangeValue(GetValue(mode, SettingsSlider.Property.ClonesAmount));
        sliders[15].ChangeValue(GetValue(mode, SettingsSlider.Property.BumperDuration));
        sliders[16].ChangeValue(GetValue(mode, SettingsSlider.Property.BumperLaunchSpeed));
        sliders[17].ChangeValue(GetValue(mode, SettingsSlider.Property.BumperBumpForce));
        sliders[18].ChangeValue(GetValue(mode, SettingsSlider.Property.TeleportDelay));
        sliders[19].ChangeValue(GetValue(mode, SettingsSlider.Property.MegaDuration));
        sliders[20].ChangeValue(GetValue(mode, SettingsSlider.Property.MegaMassIncrease));
        sliders[21].ChangeValue(GetValue(mode, SettingsSlider.Property.HijackDuration));
        sliders[22].ChangeValue(GetValue(mode, SettingsSlider.Property.HijackMoveForce));
        sliders[23].ChangeValue(GetValue(mode, SettingsSlider.Property.PlayerThrustForce));
        sliders[24].ChangeValue(GetValue(mode, SettingsSlider.Property.PlayerBoostForce));
        sliders[25].ChangeValue(GetValue(mode, SettingsSlider.Property.PlayerTurnSpeed));
        sliders[26].ChangeValue(GetValue(mode, SettingsSlider.Property.PlayerMass));
        sliders[27].ChangeValue(GetValue(mode, SettingsSlider.Property.PlayerDrag));
        sliders[28].ChangeValue(GetValue(mode, SettingsSlider.Property.PlayerDemolishThreshold));
        sliders[29].ChangeValue(GetValue(mode, SettingsSlider.Property.PlayerBumpForce));
        sliders[30].ChangeValue(GetValue(mode, SettingsSlider.Property.BallMass));
        sliders[31].ChangeValue(GetValue(mode, SettingsSlider.Property.BallDrag));
        sliders[32].ChangeValue(GetValue(mode, SettingsSlider.Property.BallBounciness));

        quickModeText.text = mode.modeName;
        PlayerPrefs.SetString("Gamemode Name", mode.modeName);
        PlayerPrefs.SetInt("Quickmode Index", quickModeIndex);
        changingQuickMode = false;
    }

    // this is an
    public float GetValue(CustomGamemode mode, SettingsSlider.Property property)
    {
        float value = 0;

        switch (property)
        {
            case SettingsSlider.Property.PowerupCooldown: value = mode.powerupCooldown; break;
            case SettingsSlider.Property.RespawnCooldown: value = mode.respawnCooldown; break;
            case SettingsSlider.Property.BoostRespawnTime: value = mode.boostRespawnTime; break;
            case SettingsSlider.Property.KickoffBoostAmount: value = mode.kickoffBoostAmount; break;
            case SettingsSlider.Property.PlungerDuration: value = mode.plungerDuration; break;
            case SettingsSlider.Property.PlungerRange: value = mode.plungerRange; break;
            case SettingsSlider.Property.PlungerForce: value = mode.plungerForce; break;
            case SettingsSlider.Property.PlungerLatchSpeed: value = mode.plungerLatchSpeed; break;
            case SettingsSlider.Property.SpikesDuration: value = mode.spikesDuration; break;
            case SettingsSlider.Property.SpikesReleaseSpeed: value = mode.spikesReleaseSpeed; break;
            case SettingsSlider.Property.GrappleLatchSpeed: value = mode.grappleLatchSpeed; break;
            case SettingsSlider.Property.GrappleGrappleSpeed: value = mode.grappleGrappleSpeed; break;
            case SettingsSlider.Property.PentagonDuration: value = mode.pentagonDuration; break;
            case SettingsSlider.Property.ClonesDuration: value = mode.clonesDuration; break;
            case SettingsSlider.Property.ClonesAmount: value = mode.clonesAmount; break;
            case SettingsSlider.Property.BumperDuration: value = mode.bumperDuration; break;
            case SettingsSlider.Property.BumperLaunchSpeed: value = mode.bumperLaunchSpeed; break;
            case SettingsSlider.Property.BumperBumpForce: value = mode.bumperBumpForce; break;
            case SettingsSlider.Property.TeleportDelay: value = mode.teleportDelay; break;
            case SettingsSlider.Property.MegaDuration: value = mode.megaDuration; break;
            case SettingsSlider.Property.MegaMassIncrease: value = mode.megaMassIncrease; break;
            case SettingsSlider.Property.HijackDuration: value = mode.hijackDuration; break;
            case SettingsSlider.Property.HijackMoveForce: value = mode.hijackMoveForce; break;
            case SettingsSlider.Property.PlayerThrustForce: value = mode.playerThrustForce; break;
            case SettingsSlider.Property.PlayerBoostForce: value = mode.playerBoostForce; break;
            case SettingsSlider.Property.PlayerTurnSpeed: value = mode.playerTurnSpeed; break;
            case SettingsSlider.Property.PlayerMass: value = mode.playerMass; break;
            case SettingsSlider.Property.PlayerDrag: value = mode.playerDrag; break;
            case SettingsSlider.Property.PlayerDemolishThreshold: value = mode.playerDemolishThreshold; break; ;
            case SettingsSlider.Property.PlayerBumpForce: value = mode.playerBumpForce; break;
            case SettingsSlider.Property.BallMass: value = mode.ballMass; break;
            case SettingsSlider.Property.BallDrag: value = mode.ballDrag; break;
            case SettingsSlider.Property.BallBounciness: value = mode.ballBounciness; break;

            default:
                Debug.LogError("You need to add the " + property + "property to the switch block in SettingsSlider.");
                break;
        }

        if (value == -2) value = SettingsSlider.defaults[property];

        return value;
    }


    public void ToggleEnabledPowerups(int toggleIndex)
    {
        if (changingQuickMode || changingAllToggles) return;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            changingAllToggles = true;

            for (int i = 0; i < powerupToggles.Length; i++)
            {
                SetPowerupToggleValue(i, false);
            }

            SetPowerupToggleValue(toggleIndex, true);


            changingAllToggles = false;
        }
        else
        {
            // make sure at least 1 powerup is enabled
            int powerupsEnabled = 0;
            foreach (bool powerup in GameInfo.enabledPowerups) if (powerup) powerupsEnabled++;

            if (powerupsEnabled == 1) powerupToggles[toggleIndex].isOn = true;

            SetPowerupToggleValue(toggleIndex, powerupToggles[toggleIndex].isOn);
            //GameInfo.enabledPowerups[toggleIndex] = powerupToggles[toggleIndex].isOn;
        }
    }

    private void SetPowerupToggleValue(int index, bool value)
    {
        powerupToggles[index].isOn = value;
        GameInfo.enabledPowerups[index] = value;

        SettingChanged();
        PlayerPrefs.SetInt("Powerup" + index, value ? 1 : 0);
    }


    public void ToggleExperimentalSettings(bool showExperimentalSettings)
    {
        foreach (GameObject settings in experimentalSettings) settings.SetActive(showExperimentalSettings);
        float newHeight = showExperimentalSettings ? withExperimentalContentHeight : noExperimentalContentHeight;
        content.sizeDelta = new Vector2(content.sizeDelta.x, newHeight);

        if (showExperimentalSettings)
        {
            ToggleDemosEnabled(GameInfo.demosEnabled);
        }
    }

    public void ToggleDemosEnabled(bool toggledOn)
    {
        GameInfo.demosEnabled = toggledOn;
        friendlyDemosToggle.SetActive(toggledOn);
    }


    public void SettingChanged()
    {
        quickModeText.text = "Custom";
        PlayerPrefs.SetString("Gamemode Name", "Custom");
    }


    //public void SetPlayerPref(string key, string value)
    //{
    //    PlayerPrefs.set
    //}
}
