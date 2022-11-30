using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsToggle : MonoBehaviour
{

    private Toggle toggle;
    [HideInInspector] public Property property;

    public enum Property
    {
        PowerupFairness,
        PowerupEquality,
        DisableGoalReset,
        Experimental,
        DemosEnabled,
        FriendlyDemosEnabled,
        ShowBotCursors,
        PentagonCanBeDemolished,
        MegaCanBeDemolished,
    };

    public static Dictionary<Property, bool> defaults = new Dictionary<Property, bool>()
    {
        { Property.PowerupFairness, false },
        { Property.PowerupEquality, false },
        { Property.DisableGoalReset, false },
        { Property.Experimental, false },
        { Property.DemosEnabled, true },
        { Property.FriendlyDemosEnabled, false },
        { Property.ShowBotCursors, false },
        { Property.PentagonCanBeDemolished, true },
        { Property.MegaCanBeDemolished, false },
    };

    private ExtraSettings extraSettings;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();

        extraSettings = FindObjectOfType<ExtraSettings>();

        //toggle.isOn = defaults[property];
    }

    public void ToggleValueChanged()
    {
        SetGameInfoProperty(/*property, */toggle.isOn);
        extraSettings.SettingChanged();
    }

    public void ChangeValue(bool on)
    {
        toggle.isOn = on;
        SetGameInfoProperty(on); // if the value of the slider stays the same then it wont actually change the property
        //extraSettings.SettingChanged();
    }

    public void SetGameInfoProperty(/*Property property, */bool value)
    {
        switch (property)
        {
            case Property.PowerupFairness: GameInfo.powerupFairness = value; break;
            case Property.PowerupEquality: GameInfo.powerupEquality = value; break;
            case Property.DisableGoalReset: GameInfo.disableGoalReset = value; break;
            case Property.Experimental: extraSettings.ToggleExperimentalSettings(value); break;
            case Property.DemosEnabled: extraSettings.ToggleDemosEnabled(value); break;
            case Property.FriendlyDemosEnabled: GameInfo.friendlyDemosEnabled = value; break;
            case Property.ShowBotCursors: GameInfo.showBotCursors = value; break;
            case Property.PentagonCanBeDemolished: GameInfo.pentagonCanBeDemolished = value; break;
            case Property.MegaCanBeDemolished: GameInfo.megaCanBeDemolished = value; break;

            default:
                Debug.LogError("You need to add the " + property + "property to the switch block in SettingsToggle.");
                break;
        }

        PlayerPrefs.SetInt(property.ToString(), value ? 1 : 0);
    }

    public void LoadPlayerPref()
    {
        ChangeValue(PlayerPrefs.GetInt(property.ToString()) == 1);
    }
}
